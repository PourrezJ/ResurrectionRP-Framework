@echo off
set ClientDir=E:\Sources\AltV\Server-Beta\resources\resurrectionrp\client
set TargetDir=E:\Sources\AltV\ResurrectionRP\ResurrectionRP_Client\bin
set ProjectDir=E:\Sources\AltV\ResurrectionRP\ResurrectionRP_Client

del %TargetDir%\*.dll
del %TargetDir%\*.pdb
rmdir /S /Q %ClientDir%
mkdir %ClientDir%
mkdir %ClientDir%\cef
mkdir %ClientDir%\lib
mkdir %ClientDir%\NativeUIMenu
xcopy /E /Y %TargetDir% %ClientDir%
xcopy /E /Y %ProjectDir%\cef %ClientDir%\cef
xcopy /E /Y %ProjectDir%\lib %ClientDir%\lib
xcopy /E /Y %ProjectDir%\client\NativeUIMenu %ClientDir%\NativeUIMenu
