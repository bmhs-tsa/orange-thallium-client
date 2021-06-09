#SingleInstance force

; Imports
#Include, %A_ScriptDir%\..\utils.ahk

; Settings
WindowTitle := "Epic Games Launcher"
Executable := "C:\Program Files (x86)\Epic Games\Launcher\Portal\Binaries\Win64\EpicGamesLauncher.exe"
ExecutableName := "EpicGamesLauncher.exe"
RememberMe := False

; Get credentials
EnvGet, username, username
EnvGet, password, password
EnvGet, mfaCode, mfaCode

; Close Epic Games if already running
if (ProcessExits(ExecutableName))
{
  Process, Close, %ExecutableName%
  Sleep, 500
}

; Start Epic Games 
Run, %Executable%
WinWait, %WindowTitle%
Sleep, 4000

; Maximize Epic Games
WinActivate, %WindowTitle%
WinMaximize, %WindowTitle%

; Disable mouse (Prevent user interference)
BlockInput, MouseMove

; Center the mouse
WinGetPos,,, windowWidth, windowHeight, %title%
MouseMove, windowWidth / 2, windowHeight / 2

; Calculate the UI area
uiAreaX1 := (windowWidth / 2) - 240
uiAreaY1 := (windowHeight / 2) - 280
uiAreaX2 := (windowWidth / 2) + 240
uiAreaY2 := (windowHeight / 2) + 320
area := {x1: uiAreaX1, y1: uiAreaY1, x2: uiAreaX2, y2: uiAreaY2}

; Click on the appropriate sign in button
signinElement := GetElement(WindowTitle, area, A_ScriptDir . "\sign-in.png", 20)
MouseMove, signinElement.x, signinElement.y
MouseClick, left
Sleep, 500

; Enter the email
emailElement := GetElement(WindowTitle, area, A_ScriptDir . "\email.png", 20)
MouseMove, emailElement.x, emailElement.y
MouseClick, left
Send, {Text}%username%

; Enter the password
passwordElement := GetElement(WindowTitle, area, A_ScriptDir . "\password.png", 20)
MouseMove, passwordElement.x, passwordElement.y
MouseClick, left
Send, {Text}%password%

; Toggle the remember me checkbox if needed
rememberMeInfo := CheckboxInfo(WindowTitle, area, A_ScriptDir . "\remember-me.png", 20)
if (rememberMeInfo.found != RememberMe)
{
  MouseMove, rememberMeInfo.x, rememberMeInfo.y
  MouseClick, left
}
Sleep, 2000

; Submit
submitElement := GetElement(WindowTitle, area, A_ScriptDir . "\submit.png", 20)
MouseMove, submitElement.x, submitElement.y
MouseClick, left

; Enable mouse
Sleep, 3500
BlockInput, MouseMoveOff

; Exit the script
ExitApp

; Get checkbox information
CheckboxInfo(title, area, reference, tolerance)
{
  ; Activate the window
  WinActivate, %title%

  ; Search the window
  CoordMode, Pixel, Window
  ImageSearch, referenceX, referenceY, area.x1, area.y1, area.x2, area.y2, *%tolerance% %reference%

  ; Construct the info
  info := {found: False, x: 0, y: 0}

  ; If the reference was found, update its position
  if (ErrorLevel == 0)
  {
    ; Get the reference dimensions
    dimensions := GetDimensions(reference)

    ; Calculate the coordinates of the center of the element
    info.x := referenceX + (dimensions.width / 2)
    info.y := referenceY + (dimensions.height / 2)

    ; Update the found flag
    info.found := True
  }

  ; Return the info
  return info
}

; Manual override
^+q::

; Close the executable
Process, Close, %ExecutableName%

; Exit the script
ExitApp

Return