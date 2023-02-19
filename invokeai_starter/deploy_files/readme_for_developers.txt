This standalone was made by Sunija. It uses InvokeAI 2.3.0 . 

If you want to create a standalone (e.g. with a newer version) follow those steps for the CONDA installation (this might not work with later versions of InvokAI):
1) Install InvokeAI according to the tutorial: https://github.com/invoke-ai/InvokeAI#installation
2) Download/install https://conda.github.io/conda-pack/
3) Open an anaconda command line (as you did during the tutorial) and run "conda pack -n invokeai -o env.zip --ignore-missing-files --ignore-editable-packages --format zip"
4) Unpack the env.zip once to invokeai_standalone/env/
5) Copy your [userfolder]/.cache to invokeai_standalone/ai_cache/
6) Copy your [userfolder]/invokeai to invokeai_standalone/invokeai/
7) Double click "invokeai_starter.exe" to see if everything works. <3