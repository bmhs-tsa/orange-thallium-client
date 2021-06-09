#SingleInstance force

; Imports
#Include, %A_ScriptDir%\..\utils.ahk

; Settings
WindowTitle := "Riot Client"
Executable := "C:\Riot Games\Riot Client\RiotClientServices.exe --launch-product=valorant --launch-patchline=live"
ExecutableName := "RiotClientUx.exe"

; Get credentials
EnvGet, username, username
EnvGet, password, password
EnvGet, mfaCode, mfaCode

; Close Valorant if already running
if (ProcessExits(ExecutableName))
{
  Process, Close, %ExecutableName%
  Sleep, 500
}

; Start Valorant
Run, %Executable%
WinWait, %WindowTitle%
Sleep, 2000

; Disable mouse (Prevent user interference)
BlockInput, MouseMove

; Center the mouse and defocus the username element
WinGetPos,,, windowWidth, windowHeight, %title%
MouseMove, windowWidth / 2, windowHeight / 2
MouseClick, left

; Calculate the UI area
area := {x1: 0, y1: 0, x2: windowWidth, y2: windowHeight}

; Change to relative coordinates
CoordMode, Mouse, Window

; Enter the username
usernameElement := GetElement(WindowTitle, area, A_ScriptDir . "\username.png", 30)
MouseMove, usernameElement.x, usernameElement.y
MouseClick, left
Send, {Text}%username%

; Enter the password
passwordElement := GetElement(WindowTitle, area, A_ScriptDir . "\password.png", 30)
MouseMove, passwordElement.x, passwordElement.y
MouseClick, left
Send, {Text}%password%

; Submit
submitElement := GetElement(WindowTitle, area, A_ScriptDir . "\submit.png", 30)
MouseMove, submitElement.x, submitElement.y
MouseClick, left

; Enable the mouse
Sleep, 1500
BlockInput, MouseMoveOff

; Exit
ExitApp

; Manual override
^+q::

; Close the executable
Process, Close, %ExecutableName%

; Exit the script
ExitApp

Return