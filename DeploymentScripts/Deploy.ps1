param(
	[Parameter(Mandatory=$true)]
	[string]$taskName,
	[Parameter(Mandatory=$true)]
	[string]$taskPath,
	[Parameter(Mandatory=$true)]
	[string]$repetitionInterval, #PT1H
	[Parameter(Mandatory=$true)]
	[string]$username,
	[Parameter(Mandatory=$true)]
	[string]$password,
	[Parameter(Mandatory=$true)]
	[string]$workingDirectory,
	[Parameter(Mandatory=$true)]
	[string]$dllName,
	[Parameter(Mandatory=$false)]
	[string]$taskDescription

)

#-command "&{cd 'C:\Program Files\Exact\CIG\Etl.CorpBI'; dotnet Cig.Etl.ContractData.Runner.dll}"

Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction Ignore

$action = New-ScheduledTaskAction -Execute $dllName -WorkingDirectory $workingDirectory
$trigger = New-ScheduledTaskTrigger -Daily -At 8am -DaysInterval 1

$task = Register-ScheduledTask -Action $action -Trigger $trigger -TaskName $taskName -TaskPath $taskPath -Description $taskDescription -User $username -Password $password
#$task.Triggers.repetition.Interval = $repetitionInterval
#$task.Triggers.repetition.Duration = "P1D"

$task | Set-ScheduledTask  -User $username -Password $password