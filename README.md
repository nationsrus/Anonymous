# Anonymous

Change web.config keys and project should run fine

If using visual studio with github extension and you want to exlude your web.config from commit, you may have to run this command (file name is caps sensitive):
git update-index --assume-unchanged Web.config
git update-index --assume-unchanged Web.Debug.config
git update-index --assume-unchanged Web.Release.config
