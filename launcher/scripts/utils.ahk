; AHK utilities

; Check if a process is running
ProcessExits(name)
{
  Process, Exist, %name%
  return ErrorLevel != 0
}

; Get an UI element
GetElement(title, area, reference, tolerance)
{
  ; Activate the window
  WinActivate, %title%

  ; Search the window
  CoordMode, Pixel, Window
  ImageSearch, referenceX, referenceY, area.x1, area.y1, area.x2, area.y2, *%tolerance% %reference%

  ; Ensure the reference was found before proceding
  if (ErrorLevel != 0)
  {
    Throw, "Failed to get the UI element! (Error: " ErrorLevel ", title: " title ", tolerance: " tolerance ", P1: (" area.x1 "," area.y1 "), P2: (" area.x2 "," area.y2 "), reference: " reference ")"
  }

  ; Get the reference dimensions
  dimensions := GetDimensions(reference)

  ; Calculate the coordinates of the center of the element
  elementX := referenceX + (dimensions.width / 2)
  elementY := referenceY + (dimensions.height / 2)

  ; Return an object
	return {x: elementX, y: elementY}
}

; Get the dimensions of an image (Source: https://github.com/Masonjar13/AHK-Library/blob/master/Lib/getImageSize.ahk)
GetDimensions(path)
{
  ; Parse the path
  SplitPath, path, fileName, fileDirectory

  ; Use the explorer COM API to get the dimensions property
  rawDimensions := ComObjCreate("Shell.Application")
    .namespace(fileDirectory?fileDirectory:a_workingDir)
    .parseName(fileDirectory?fileName:path)
    .extendedProperty("Dimensions")

  ; Parse the raw dimension
	dimensions := StrSplit(rawDimensions, "x", " " chr(8234) chr(8236))

  if (dimensions.Length() != 2)
  {
    Throw, "Failed to get image dimensions! (Path: " path ")"
  }

  ; Return an object
	return {height: dimensions[2], width: dimensions[1]}
}