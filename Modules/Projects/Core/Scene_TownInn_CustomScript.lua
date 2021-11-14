-- Balancing variables
-- TO BE REMOVED
local room_price = 5
local drink_price = 1

-- On first encounter, generate a random canine species for the barkeep
if not Storage.GetFlag("barkeep_generated") then
    Storage.SetString("barkeep_species", Text.GetString("SPECIES_CANINE"))
    Storage.SetString("foxtaur_partner_species", Text.GetString("SPECIES_FELINE"))
    Storage.SetFlag("barkeep_generated", true)
end

-- Prepare text variables
local barkeep = Creature("CR_RandomAnthro")
barkeep.Name = Storage.GetString("barkeep_species")
barkeep.Alias = "the " .. Storage.GetString("barkeep_species")
barkeep.Gender = EGender.Male
local ftpartner = Storage.GetString("foxtaur_partner_species")
