-- Utility function for checking time
-- The scene is fatal if no time passed since Chip's last meal
local function IsChipVoreFatal()
    return Storage.GetNumber("MQ04_CHIP_LAST_ATE") == GetTimeHourTotal()
end

function ChipCVFatalCheck()
    return Storage.GetNumber ("WRC_CHIP_CVRELEASE") < GetTimeHourTotal()+4
end