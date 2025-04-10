﻿namespace Entities.Web.Testing.Infrastructure;

public static class PersonNames
{
    public static (string GivenName, string FamilyName)[] NL =>
    [
        ("Klaar", "Kome"),
        ("Connie", "Comen"),
        ("Connie", "Plassen"),
        ("Beau", "ter Ham"),
        ("Con", "Domen"),
        ("Justin", "Case"),
        ("Dick", "Sack"),
        ("Bennie", "Dood"),
        ("Sam", "Bal"),
        ("Alain", "Terieur"),
        ("Alex", "Terieur"),
        ("Wil", "de Gans"),
        ("Bennie", "Thuis"),
        ("Dick", "Tevreden"),
        ("Joska", "Bouter"),
        ("Piet", "Uyttebroeck"),
        ("Mario", "Netten"),
        ("Klaar", "Wakker"),
        ("Sjef", "Kok"),
        ("Matt", "Ras"),
        ("Mette", "Bus"),
        ("Nies", "Slim"),
        ("Lieve", "Lingen"),
        ("Dino", "Saris"),
        ("Ferry", "Kok"),
        ("Peter", "Selis"),
        ("Elke", "Kant")
    ];
    public static (string GivenName, string FamilyName)[] EN =>
    [
        ("Zoowee", "Blubberworth"),
        ("Flufffy", "Gloomkins"),
        ("Buritt", "Noseface"),
        ("Peaberry", "Wigglewhistle"),
        ("Trashwee", "Sockborn"),
        ("Flapberry", "Fudgewhistle"),
        ("Gummoo", "Hooperbottom"),
        ("Humster", "Pottyworthy"),
        ("Bugby", "Doodoohill"),
        ("Gootu", "Snotborn"),
        ("Peafy", "Doodoofish"),
        ("Peawee", "Pimplehair"),
        ("Chewlu", "Boogerbrain"),
        ("Eggster", "HoboSmittens"),
        ("Bushspitz", "Wigglebottom"),
        ("Stinkroid", "Noodleshine"),
        ("Hicktu", "Sockface"),
        ("Chewberry", "Mudman"),
        ("Sniffeenie", "Chewgold"),
        ("Fartbag", "Snotshine"),
        ("Eggpants", "Snothall"),
        ("Dingbo", "Roachseed"),
        ("Zoobo", "Hobogold"),
        ("Gumbs", "Beaniebag"),
        ("Shuwee", "Woofham"),
        ("Sniffdori", "Toothair"),
        ("Bugcan", "Noodleworthy"),
        ("Sniffwee", "Gloomshine"),
        ("Burberry", "Gloomhall"),
        ("Slugtu", "Hooperseed"),
        ("Bofy", "Gloomman"),
        ("Sniffpants", "Pimplehill"),
        ("Stinkmoo", "Snotsniff"),
        ("Barfberry", "Oinkhill"),
        ("Barfaboo", "Roachson"),
        ("Wormbag", "Chewface"),
        ("Peaster", "Beeman"),
        ("Figitt", "Tootson"),
        ("Subo", "Doozyton"),
        ("Pea-a-boo", "Moanihill"),
        ("Zoobuns", "Sockball"),
        ("Foobug", "Swamprider"),
        ("Peamoo", "Fudgehill"),
        ("Ratbuns", "Wigglefish"),
        ("Madaloo", "Snotseed"),
        ("Peabs", "Blubberseed"),
        ("Bushbo", "Tootbean"),
        ("Binfy", "Moanigold"),
        ("Bittyspitz", "Boogerfeet"),
        ("Fartmoo", "PimpleFadden"),
        ("Binpants", "Noodlefeet"),
        ("Chewspitz", "Snotborn"),
        ("Gootu", "Messyhall"),
        ("Hickwee", "Roachworth"),
        ("Sniffwax", "Pukeseed"),
        ("Weebee", "Tootseed"),
        ("Bittymoo", "FudgeSmittens"),
        ("Shoospitz", "Droopyface"),
        ("Buzzwee", "Hobobag"),
        ("Zoowee", "Beeham"),
        ("Monipants", "Woofkins"),
        ("Cooitt", "Noseseed"),
        ("Fuzzman", "Pottyman"),
        ("Bur-a-boo", "Noodleborn"),
        ("Tummoo", "Boombottom"),
        ("Whee-a-boo", "Wiggleton"),
        ("Shoofy", "Hoboman"),
        ("Flapbuns", "WiggleFadden"),
        ("Bushwee", "Chewshine"),
        ("Borebo", "Madworthy"),
        ("Ratspitz", "Noodlehill"),
        ("Tumspitz", "Spottyhair"),
        ("Hickcan", "Woolhall"),
        ("Stinkroid", "Noodlebrain"),
        ("Buzzlu", "Boombag"),
        ("Bugbs", "Woolkins"),
        ("Bugbs", "Pimpleson"),
        ("Snifflu", "HippySmittens"),
        ("Gooster", "Bobohair"),
        ("Snoobuns", "Hoboworth"),
        ("Bumeenie", "Moaniman"),
        ("Bobuns", "Pimplehill"),
        ("Pooritt", "Woofhair"),
        ("Stinkwee", "Goatbottom"),
        ("Chewitt", "Pottybag"),
        ("Snortu", "Madborn"),
        ("Fartaloo", "Roachhall"),
        ("Bugbee", "Noseson"),
        ("Poopfy", "Chewbrain"),
        ("Zoozy", "Beaniehall"),
        ("Trashbug", "Messyhall"),
        ("Peaman", "Oinkson"),
        ("Stinkbuns", "Spottygold"),
        ("Bittypants", "Messyrider"),
        ("Bugeenie", "Roachson"),
        ("Dingbs", "Moonbean"),
        ("Snabster", "Toothill"),
        ("Peawee", "Tootwhistle"),
        ("Eggbo", "Roachshine"),
        ("Bincan", "Moongold")
    ];

    public static IEnumerable<string> GivenNames => EN.Select(x => x.GivenName);
    public static IEnumerable<string> FamilyNames => EN.Select(x => x.FamilyName);
}