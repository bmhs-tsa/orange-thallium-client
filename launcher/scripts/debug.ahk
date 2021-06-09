#SingleInstance force

; Get credentials
EnvGet, username, username
EnvGet, password, password
EnvGet, mfaCode, mfaCode

; Display
MsgBox, Username: %username% Password: %password% MFA Code: %mfaCode%

; Exit the script
ExitApp

; Manual override
^+q::

; Exit the script
ExitApp

Return