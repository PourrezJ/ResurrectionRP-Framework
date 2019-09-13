@echo off
set ClientDir=C:\altv\server-files\resources\resurrectionrp\client
set TargetDir=C:\Users\Administrateur\source\repos\ALTV_ResurrectionRP\ResurrectionRP_Client\bin
set ProjectDir=C:\Users\Administrateur\source\repos\ALTV_ResurrectionRP\ResurrectionRP_Client

del %TargetDir%\*.dll
del %TargetDir%\*.pdb
rmdir /S /Q %ClientDir%
mkdir %ClientDir%
mkdir %ClientDir%\cef
mkdir %ClientDir%\lib
mkdir %ClientDir%\NativeUIMenu
mkdir %ClientDir%\Streamer
xcopy /E /Y %TargetDir% %ClientDir%
xcopy /E /Y %ProjectDir%\cef %ClientDir%\cef
xcopy /E /Y %ProjectDir%\lib %ClientDir%\lib
xcopy /E /Y %ProjectDir%\client\NativeUIMenu %ClientDir%\NativeUIMenu
xcopy /E /Y %ProjectDir%\client\Streamer %ClientDir%\Streamer