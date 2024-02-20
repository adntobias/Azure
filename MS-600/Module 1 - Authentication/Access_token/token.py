#pip install flask
#set FLASK_APP=app.py -> flask run
#http://localhost:5000


from flask import Flask, redirect, request
from requests import post
from urllib.parse import urlencode
from os import environ
from dotenv import load_dotenv

load_dotenv()

app = Flask(__name__)

access_token = ""

@app.route("/logout")
def logout():
    global access_token
    access_token = ""
    return redirect(
        f"https://login.microsoftonline.com/{environ.get('TENANT_ID')}/oauth2/v2.0/logout?post_logout_redirect_uri={environ.get('BASE_URL')}"
    )

@app.route("/login")
def login():
    return redirect(
        f"https://login.microsoftonline.com/{environ.get('TENANT_ID')}/oauth2/v2.0/authorize?client_id={environ.get('CLIENT_ID')}"
        f"&response_type=code&redirect_uri={environ.get('BASE_URL')}{environ.get('REDIRECT_URL')}"
        f"&response_mode=query&state=&scope={environ.get('SCOPE')}&prompt=consent"
    )

@app.route(environ.get('REDIRECT_URL'))
def redirect_url():
    global access_token
    auth_code = request.args.get('code')

    if not auth_code:
        return "There was no authorization code provided in the query. No Bearer token can be requested", 500

    data = {
        'grant_type': 'authorization_code',
        'code': auth_code,
        'client_id': environ.get('CLIENT_ID'),
        'scope': environ.get('SCOPE'),
        'client_secret': environ.get('CLIENT_SECRET'),
        'redirect_uri': f"{environ.get('BASE_URL')}{environ.get('REDIRECT_URL')}"
    }

    response = post(f"https://login.microsoftonline.com/{environ.get('TENANT_ID')}/oauth2/v2.0/token", data)

    if response.json().get('error'):
        return f"Error occured: {response.json().get('error')}\n{response.json().get('error_description')}", 500
    else:
        access_token = "Bearer " + response.json().get('access_token')
        return redirect("/")

@app.route('/')
def home():
    if access_token:
        return f'<a href="{environ.get("BASE_URL")}/logout">LOGOUT</a><br/>You got your token!! <br><div style="display: block;max-width:95%;word-wrap: break-word">{access_token}</div>'
    return f'<a href="{environ.get("BASE_URL")}/login">LOGIN</a>'

if __name__ == "__main__":
    app.run(port=5001)