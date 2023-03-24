@ECHO OFF

ECHO: Try updating. This is an alpha! No guarantee that the update will work.

call "%cd%/env/Scripts/activate.bat"

set "TRANSFORMERS_CACHE=%cd%/ai_cache/huggingface/transformers"
set "TORCH_HOME=%cd%/ai_cache/torch"

"%cd%/env/python.exe" "%cd%/env/Scripts/invokeai-update.exe" --root "%cd%/invokeai/"
pause
exit


