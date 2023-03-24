@ECHO OFF

ECHO: Start configurator to install more models.

call %cd%/env/Scripts/activate.bat

set "TRANSFORMERS_CACHE=%cd%/ai_cache/huggingface/transformers"
set "TORCH_HOME=%cd%/ai_cache/torch"

"%cd%/env/python.exe" "%cd%/env/Scripts/invokeai-configure.exe" --root "%cd%/invokeai/"
pause
exit


