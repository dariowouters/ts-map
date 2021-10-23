param ($project)

Write-Host "Generating Build Number for project $project"

#Get the version from the csproj file
$xml = [Xml] (Get-Content $project\$project.csproj)
$initialVersion = [Version] $xml.Project.PropertyGroup.AssemblyVersion

#Get the build number (number of days since January 1, 2000)
$baseDate = [datetime]"01/01/2000"
$currentDate = $(Get-Date)
$interval = (NEW-TIMESPAN -Start $baseDate -End $currentDate)
$buildNumber = $interval.Days

#Get the revision number (number seconds (divided by two) into the day on which the compilation was performed)
$StartDate=[datetime]::Today
$EndDate=(GET-DATE)
$revisionNumber = [math]::Round((New-TimeSpan -Start $StartDate -End $EndDate).TotalSeconds / 2,0)

#Final version number
$finalBuildVersion = "$($initialVersion.Major).$($initialVersion.Minor).$($buildNumber).$($revisionNumber)"
Write-Host "Final build number: " $finalBuildVersion

$workingDirectory = Get-Location
$publishDirectory = "$workingDirectory\$project\bin\Publish\"
Write-Host "Publish folder: $publishDirectory"

Remove-Item "$publishDirectory*"
dotnet publish $project -o $publishDirectory -c Release --nologo -p:Version=$($finalBuildVersion) -p:AssemblyVersion=$($finalBuildVersion)
Start-Sleep -s 1
Rename-Item "$publishDirectory$project.exe" "$publishDirectory$project-$finalBuildVersion.exe" 