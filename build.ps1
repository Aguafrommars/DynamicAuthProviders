$result = 0

if ($isLinux) {
  dotnet build -c Release
	
	Get-ChildItem -rec `
	| Where-Object { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| ForEach-Object { 
		Set-Location $_.DirectoryName
		
		dotnet test -c Release --no-build
	
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	}
} else {
	$prNumber = $env:APPVEYOR_PULL_REQUEST_NUMBER
	if ($prNumber) {
        $prArgs = "-d:sonar.pullrequest.key=$prNumber"
    } elseif ($env:APPVEYOR_REPO_BRANCH) {
        $prArgs = "-d:sonar.branch.name=$env:APPVEYOR_REPO_BRANCH"
    }

	if (-Not $env:APPVEYOR_PULL_REQUEST_NUMBER) {
		dotnet sonarscanner begin /k:aguacongas_DymamicAuthProviders -o:aguacongas -d:sonar.host.url=https://sonarcloud.io -d:sonar.login=$env:sonarqube -d:sonar.coverageReportPaths=coverage\SonarQube.xml $prArgs -v:$env:Version
	} elseif ($env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME -eq $env:APPVEYOR_REPO_NAME) {
		dotnet sonarscanner begin /k:aguacongas_DymamicAuthProviders -o:aguacongas -d:sonar.host.url=https://sonarcloud.io -d:sonar.login=$env:sonarqube -d:sonar.coverageReportPaths=coverage\SonarQube.xml $prArgs -v:$env:Version
	}

	dotnet build -c Release

	Get-ChildItem -rec `
	| Where-Object { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| ForEach-Object { 
        &('dotnet') ('test', $_.FullName, '--logger', "trx;LogFileName=$_.trx", '--no-build', '-c', 'Release', '--collect:"XPlat Code Coverage"')    
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	  }

	$merge = ""
	Get-ChildItem -rec `
	| Where-Object { $_.Name -like "coverage.cobertura.xml" } `
	| ForEach-Object { 
		$path = $_.FullName
		$merge = "$merge;$path"
	}
	Write-Host $merge
	ReportGenerator\tools\net6.0\ReportGenerator.exe "-reports:$merge" "-targetdir:coverage" "-reporttypes:SonarQube"
	
	if (-Not $env:APPVEYOR_PULL_REQUEST_NUMBER) {
		dotnet sonarscanner end -d:sonar.login=$env:sonarqube
	} elseif ($env:APPVEYOR_PULL_REQUEST_HEAD_REPO_NAME -eq $env:APPVEYOR_REPO_NAME) {
		dotnet sonarscanner end -d:sonar.login=$env:sonarqube
	}
}
exit $result

  
