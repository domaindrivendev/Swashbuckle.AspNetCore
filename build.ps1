Import-Module .\tools\psake\psake.psm1
Invoke-psake .\build-definition.ps1

# Get Psake to Return Non-Zero Return Code on Build Failure (https://github.com/psake/psake/issues/58)
exit !($psake.build_success)

