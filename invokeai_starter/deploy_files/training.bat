@ECHO OFF

:start

ECHO: 
ECHO  __     __   __     __   __   ______     __  __     ______     ______     __    
ECHO /\ \   /\ "-.\ \   /\ \ / /  /\  __ \   /\ \/ /    /\  ___\   /\  __ \   /\ \   
ECHO \ \ \  \ \ \-.  \  \ \ \'/   \ \ \/\ \  \ \  _"-.  \ \  __\   \ \  __ \  \ \ \  
ECHO  \ \_\  \ \_\\"\_\  \ \__|    \ \_____\  \ \_\ \_\  \ \_____\  \ \_\ \_\  \ \_\ 
ECHO   \/_/   \/_/ \/_/   \/_/      \/_____/   \/_/\/_/   \/_____/   \/_/\/_/   \/_/ 
ECHO:                                                                               
ECHO:
ECHO Standalone by Sunija.
ECHO:
ECHO Loading Training Interface...


call "%cd%/env/Scripts/activate.bat"

set "TRANSFORMERS_CACHE=%cd%/ai_cache/huggingface/transformers"
set "TORCH_HOME=%cd%/ai_cache/torch"

"%cd%/env/python.exe" "%cd%/env/Scripts/invokeai-ti.exe" --gui --root "%cd%/invokeai/"
pause
exit


