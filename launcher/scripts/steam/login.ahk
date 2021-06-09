#SingleInstance force

; Imports
#Include, %A_ScriptDir%\..\utils.ahk

; Settings
Executable := "C:\Program Files (x86)\Steam\steam.exe"
ExecutableName := "steam.exe"

; Get credentials
EnvGet, username, username
EnvGet, password, password
EnvGet, mfaCode, mfaCode

; Close Steam if already running
if (ProcessExits(ExecutableName))
{
  Process, Close, %ExecutableName%
}

; Start Steam with appropriate arguments
Run, %Executable% -login %username% %password%

; Exit the script
ExitApp

; Manual override
^+q::

; Close the executable
Process, Close, %ExecutableName%

; Exit the script
ExitApp

Return