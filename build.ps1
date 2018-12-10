$result = 0

if ($isLinux) {
	gci -rec `
	| ? { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| % { 
		cd $_.DirectoryName
		dotnet test
	
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	}
} else {
	gci -rec `
	| ? { $_.Name -like "*.IntegrationTest.csproj" `
		   -Or $_.Name -like "*.Test.csproj" `
		 } `
	| % { 
        &('dotnet') ('test', $_.FullName, '--logger', "trx;LogFileName=$_.trx", '-c', 'Release', '/p:CollectCoverage=true', '/p:CoverletOutputFormat=cobertura')    
		if ($LASTEXITCODE -ne 0) {
			$result = $LASTEXITCODE
		}
	  }

    $merge = ""
    gci -rec `
    | ? { $_.Name -like "coverage.cobertura.xml" } `
    | % { 
        $path = $_.FullName
        $merge = "$merge;$path"
    }
    Write-Host $merge
    ReportGenerator\tools\net47\ReportGenerator.exe "-reports:$merge" "-targetdir:coverage\docs" "-reporttypes:HtmlInline;Badges"
}
exit $result
  