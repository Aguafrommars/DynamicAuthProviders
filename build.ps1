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
        &('dotnet') ('test', $_.FullName, '--logger', "trx;LogFileName=$_.trx", '-c', 'Release', '/p:CollectCoverage=true', '/p:CoverletOutputFormat=cobertura')    
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
    ReportGenerator\tools\net47\ReportGenerator.exe "-reports:$merge" "-targetdir:coverage\docs" "-reporttypes:HtmlInline;Badges"
}
exit $result
  