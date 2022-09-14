dotnet publish -c release -r win-x64 /p:PublishSingleFile=true
dotnet publish -c release -r osx-x64 /p:PublishSingleFile=true
dotnet publish -c release -r linux-x64 /p:PublishSingleFile=true

copy-item ./bin/release/netcoreapp3.0/win-x64/publish/Generate-NACException.exe ./bin/
copy-item ./bin/release/netcoreapp3.0/osx-x64/publish/Generate-NACException ./bin/Generate-NACException-OSX
copy-item ./bin/release/netcoreapp3.0/linux-x64/publish/Generate-NACException ./bin/Generate-NACException-Linux