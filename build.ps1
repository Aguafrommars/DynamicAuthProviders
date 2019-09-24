$result = 0

if ($isLinux) {
	Get-ChildItem -rec `
	| Where-Object { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| ForEach-Object { 
		Set-Location $_.DirectoryName
		dotnet test
	
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	}
} else {
	Get-ChildItem -rec `
	| Where-Object { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| ForEach-Object { 
        &('dotnet') ('test', $_.FullName, '--logger', "trx;LogFileName=$_.trx", '-c', 'Release')    
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	  }
}
exit $result
  