# invokeai_starter
A standalone starter for InvokeAI.

![invokeai_standalone_starter](https://github.com/GothaB/invokeai_starter/assets/10253311/c2d141be-7adc-471d-9738-deba2b611761)

# How to build it yourself
TL;DR: We install invokeai in a conda-environment and make it standalone by using conda-pack. Then we add some bat-scripts and a simple UI (written in WPF C#) so it's easy to use.

## Build the starter
1. Clone the repo
2. Open it in Visual Studio (it's a WPF project)
3. Build it (in x64, because x32 somehow doesn't work)
4. Copy the built version to your new standalone folder
5. Download the standalone and copy the .bat files to your standalone. Sorry for the hassle, but I didn't push themm yet. :( The most important is the helper.bat (it starts the standalone) which looks like that:
```
@ECHO OFF
call "%cd%/env/Scripts/activate.bat"
"%cd%/env/python.exe" "%cd%/env/Scripts/invokeai-web.exe" --root "%cd%/invokeai/"
pause
exit
```


## Make invokeai standalone

### Install invokeai via conda
1. Prerequisites: Install python 3.10, anaconda and [conda-pack](https://conda.github.io/conda-pack/)
2. Install invokeai via conda. You can find the instructions [here](https://invoke-ai.github.io/InvokeAI/installation/020_INSTALL_MANUAL/#unsupported-conda-install). If that site is not available anymore, I post the latest instructions here: 
```console
mkdir ~/invokeai
conda create -n invokeai python=3.10
conda activate invokeai
pip install InvokeAI[xformers] --use-pep517 --extra-index-url https://download.pytorch.org/whl/cu117
invokeai-configure --root ~/invokeai
invokeai --root ~/invokeai --web
```

### Create the standalone environment
1. Copy the invokeai folder (in your User folder) to your new standalone folder
2. Open an anaconda command line (just type "anaconda" in the windows search) and run the following command to pack the invokeai environment:
```console
conda pack -n invokeai -o invokeai_packed_env.zip --ignore-missing-files --ignore-editable-packages --format zip
```
3. Copy the newly created /UserFolder/invokeai_packed_env.zip to in /your_standalone_folder/env/ and unzip it there. Delete the zip afterwards to save space.

### Check that your standalone is actually standalone
1. Get rid of anything on your system that could be used to run invokeai, except your standalone. E.g. delete/rename the anaconda environment (in User/anaconda/envs), rename %appdata%/python, get rid of your invokeai folder in User/invokeai, move your standalone somewhere else, etc
2. Try to run the standalone

### Update your standalone
1. In the best case, the update.bat (which uses invokeai's update script) works. Then you can use it, and you're done.
2. ...otherwise, you have to do the steps above from scratch.
