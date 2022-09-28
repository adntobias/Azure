#Prevents the computer from falling asleep
#Opens a notepad and writes a "." every second and runs until specified time runs out or notepad has been closed



#############
# Functions #
#############

# User inputs amount of minutes they want to go for
# validates user input 0-9
function userInput
{
    do{
        $input = -1 #inital parameter, input is false(-1)
        $minutes = Read-Host "Please enter number of minutes to stay awake"

        #Check to see if input is correct
        if($minutes -notmatch "[0-9]"){
            Write-Host ""
            Write-Host "Warning: Only use number 0 through 9"
            Write-Host ""
            $input = -1
        }else{
            $input = 1
            $timer = 60 * $minutes #convert seconds to minutes
        }
    }while($input -ne 1)
   
    return $timer
}

function startNotepad
{
    # Opens new notepad
    return Start-Process notepad -PassThru -WindowStyle Normal 
}

function exitProg
{
    #Read-Host "Press ENTER key to exit..."
    Write-Host "Good Bye!"
    exit
}

#Writes "." to notepad until notepad has closed
function writeToNotepad
{
    $count = 0
    #Continue pressing the keypad while Notepad is still open or the timer has not run out.
    while(-not $notepad.HasExited -and ($timer -ge 0)){
            Start-Sleep -Seconds 1 
            #Convert from seconds to HH:MM:SS
            $ts =  [timespan]::fromseconds($timer)
            Write-Host Time Remaining: $ts
            #Counters
            $timer--
            $count++
            #Before keypress check to make sure Notepad has not been closed and press key every specified number of seconds. Not to spam 
            if((-not $notepad.HasExited) -and ($count -eq 30)){           
                $myshell.sendkeys(".")
                $count = 0
            }
    }
}


############
#   Main   #
############

$timer = userInput #Set time to user input
# Keep computer awake until notepad is terminated, the ask if user wants to continue
do{
    $notepad = startNotepad #Start notepad if it does not exist
    $myshell = New-Object -com "Wscript.Shell"
    writeToNotepad

    #User input is required to close console
    $date = Get-Date -Format g
    if($notepad.HasExited){
        Stop-Process $notepad
        Write-Host -ForegroundColor Red "Script has been terminated early. Script terminated on  " $date
    }else{
        Write-Host -ForegroundColor Green "Timer has expired on " $date
    }
    Write-Host -ForegroundColor DarkYellow "If you would like to continue please enter new number or enter 0 to exit"
    $timer = userInput
}while( $timer -ne 0)

# Exit program
exitProg

