const express = require('express')
const morgan = require('morgan')
const axios = require('axios')
const qs = require('qs')
require('dotenv').config()

const PORT = 5001
const app = express()
app.use(express.json())
app.use(morgan('dev'))

let access_token = ""
//Logout redirect to Microsoft logout page
app.get("/logout", (req, res) => {
    access_token = "";
    res.redirect(
        "https://login.microsoftonline.com/" + process.env.TENANT_ID + "/oauth2/v2.0/logout?" +
        "post_logout_redirect_uri=" + process.env.BASE_URL         
        );
});

//Weiterleitung zur Microsoft login page
app.get("/login", (req, res) => {
    res.redirect(
      //[Tenant ID] Für Multi-Tenant -> "common"  (Jeder mit MS-Konto kann sich einloggen)
      //[Version] Derzeit gibt es v1.0 und v2.0. v1.0 erlaubt nur Unternehmenskonten, v2.0 Unternehmens- und Privatkonten
      "https://login.microsoftonline.com/" + process.env.TENANT_ID + "/oauth2/v2.0/authorize?" +
      //[client_id] App ID aus dem Azure AD
      "client_id=" + process.env.CLIENT_ID +
      //[response_type] Spezifiziert den Return-Wert des ADs bei Erfolg. Entweder "code" für den OAuth 2.0 flow oder "id_token" für den OpenIdConnect flow 
      "&response_type=code" +
      "&redirect_uri=" + process.env.BASE_URL + process.env.REDIRECT_URL +
      //[response_mode] Gibt die Methode an wie der Login erfolgt. Es kann "query", "form_post" oder "fragment" angegeben werden
      "&response_mode=query" +
      //[state] Optionaler Wert, kann z.b. weitere Infos an die App weitergeben bei Login
      "&state= " +
      //[scope] Die angeforderten Rechte der Applikation
      "&scope=" + process.env.SCOPE + // "https://graph.microsoft.com/.default" 
      "&prompt=consent"
      //Optionale Parameter
      //[Prompt] Kann "login", "consent" oder "none" sein, spezifiziert den Prompt an den User beim Login
      //[Login_hint] Auto-fill des Usernamens oder Email bei Loginverfahren
      //[Domain_hint] Kann "consumers" oder "organizations" sein, bestimmt welche Accounttypen zulässig sind
    );
});

app.get(process.env.REDIRECT_URL, async (req, res) => {
    const authCode = String(req.query.code)

    console.log(req.query)
    
    if (!authCode) {
        return res.status(500).send("There was no authorization code provided in the query. No Bearer token can be requested");
    }

    const data = qs.stringify({
        //[grant_type]: Spezifiziert die Nutzung eines Bearer-Tokens
        grant_type: "authorization_code",
        //[code] Code vom Auth-Call der gegen den Access Token eingetauscht wird
        code: authCode,
        //[client_id] Die App ID aus dem Azure AD 
        client_id: process.env.CLIENT_ID,
        scope: process.env.SCOPE,
        //[client_secret] Das Client Secret aus dem Azure AD 
        client_secret: process.env.CLIENT_SECRET,
        //[redirect_uri] Die URL, die von MS mit dem Access Token aufgerufen wird
        redirect_uri: process.env.BASE_URL + process.env.REDIRECT_URL
    })

    try {
        const response = await axios.post("https://login.microsoftonline.com/" + process.env.TENANT_ID + "/oauth2/v2.0/token", data)

        if (response.error) { 
            res.status(500).send("Error occured: " + response.error + "\n" + response.error_description)
        }
        else {
            access_token = "Bearer "+response.data.access_token
            res.redirect("/")
        }
    } catch (error) {
        console.log(error);
    }
});

app.get('*', (req, res) => {
    if(access_token) {
        return res.send('<a href="'+process.env.BASE_URL+'/logout">LOGOUT</a><br/>You got your token!! <br><div style="display: block;max-width:95%;word-wrap: break-word">' + access_token+'</div>')
    }

    res.send('<a href="'+process.env.BASE_URL+'/login">LOGIN</a>')
})

app.listen(PORT, () => {
    console.log(`Listen on http://localhost:${PORT}`);
})