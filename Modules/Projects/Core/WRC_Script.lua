function ChipCVFatalCheck()
    return Storage.GetNumber ("WRC_CHIP_CVRELEASE") < GetTimeHourTotal()+4
end