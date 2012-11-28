if ($args.count -eq 0) {
	$rnd = Get-Random -minimum 0 -maximum 100
	Write-Host "rnd.value" $rnd
	Write-Host "."
	exit 0
}

if ($args[0] -eq "config") {
	Write-Host "graph_title Random number test"
	Write-Host "graph_category test"
	Write-Host "graph_args --upper-limit 100 -l 0"
	Write-Host "rnd.label Random number"
	Write-Host "."
	exit 0
}

Write-Host "."
exit 0
