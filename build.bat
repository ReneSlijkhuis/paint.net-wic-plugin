rem - Use: Developer Command Prompt VS2015

cd src
MSBuild WicDecoder.sln /t:Rebuild /p:Configuration=Release

cd bin
cd Release
7za a -tzip WicDecoder.zip WicDecoder.dll
7za a -tzip WicDecoder.zip ..\..\..\config\WicDecoder.ini

cd ..
cd ..
cd ..
