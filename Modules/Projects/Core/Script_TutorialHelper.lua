function ApplyPlayerStartingGear()
    -- Remove tutorial prey, if any
    Player.PredatorFullness = 0
    Player.TotalPreySwallowed = 0
    Player.TotalPreyDigested = 0

    -- Assign starting equipment
    Player.Money = 7
    Player.EquippedArmor = Item("A_ClothArmor")
    Player.EquippedAccessory1 = Item("AC_IsoRing")

    -- If Explorer Mode is on, give the player a 'cheating' ring
    if Player.IsExplorerModeEnabled then
        Player.EquippedAccessory2 = Item("AC_ExplorerModeRing")
    end
end

function BeginGame()
    ApplyPlayerStartingGear()

    -- Opening text
    ClearLog()
    Log("opening")
    Sleep(2.5)

    -- Warp to game area
    SetScene("Scene_TownMarket")
end
