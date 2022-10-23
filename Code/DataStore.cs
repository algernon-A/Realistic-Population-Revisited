// <copyright file="DataStore.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RealPop2
{
    using System.Collections.Generic;

    /// <summary>
    /// The legacy data store.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Legacy code")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Legacy code")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Legacy code")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Legacy code")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Visual alignment")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1500:Braces for multi-line statements should not share line", Justification = "Visual alignment")]
    public class DataStore
    {
        public const int CACHE_SIZE = 32768;

        // Array indexes
        // TODO - Turn into structs
        public const int PEOPLE = 0;  // sqm per person, also calculated value for internal use only
        public const int LEVEL_HEIGHT = 1;  // m per floor.
        public const int DENSIFICATION = 2;
        public const int CALC_METHOD = 3;  // 0 for model, 1 for plot
        public const int VISIT = 4;

        public const int WORK_LVL0 = 5;
        public const int WORK_LVL1 = WORK_LVL0 + 1; // 6
        public const int WORK_LVL2 = WORK_LVL1 + 1; // 7
        public const int WORK_LVL3 = WORK_LVL2 + 1; // 8

        public const int POWER = 9;
        public const int WATER = POWER + 1; // 10
        public const int SEWAGE = WATER + 1; // 11
        public const int GARBAGE = SEWAGE + 1; // 12
        public const int INCOME = GARBAGE + 1; // 13

        public const int GROUND_POLLUTION = 14;
        public const int NOISE_POLLUTION = GROUND_POLLUTION + 1; // 15

        public const int PRODUCTION = 16;
        public const int MAIL = 17;

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        public static string currentFileLocation = string.Empty;

        // Flags in XML
        public static bool enableExperimental = false;
        public static bool strictCapacity = true;

        // Static as required for functionality in EnsureCitizenUnits, but after loading the city
        public static bool allowRemovalOfCitizens = false;

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    8, 20, 15, 11, 130,   0, 1,   -1, 35 },
                                                 new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    8, 21, 16, 10, 140,   0, 1,   -1, 30 },
                                                 new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    9, 22, 17, 10, 150,   0, 1,   -1, 25 },
                                                 new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    9, 24, 19,  9, 160,   0, 1,   -1, 20 },
                                                 new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,   10, 26, 21,  9, 170,   0, 1,   -1, 15 }, };

        public static int[][] residentialHigh = { new int[] { 140, 5, -1, 0, -1,   -1, -1, -1, -1,    7, 14, 11, 9, 90,   0, 5,   -1, 25 },
                                                  new int[] { 145, 5, -1, 0, -1,   -1, -1, -1, -1,    7, 15, 12, 8, 90,   0, 5,   -1, 20 },
                                                  new int[] { 150, 5, -1, 0, -1,   -1, -1, -1, -1,    8, 16, 13, 8, 90,   0, 5,   -1, 16 },
                                                  new int[] { 160, 5, -1, 0, -1,   -1, -1, -1, -1,    8, 17, 14, 7, 90,   0, 5,   -1, 12 },
                                                  new int[] { 170, 5, -1, 0, -1,   -1, -1, -1, -1,    9, 19, 16, 7, 90,   0, 5,   -1,  8 }, };

        public static int[][] residentialW2W = { new int[] { 140, 5, -1, 0, -1,   -1, -1, -1, -1,    7, 14, 11, 9, 90,   0, 5,   -1, 25 },
                                                  new int[] { 145, 5, -1, 0, -1,   -1, -1, -1, -1,    7, 15, 12, 8, 90,   0, 5,   -1, 20 },
                                                  new int[] { 150, 5, -1, 0, -1,   -1, -1, -1, -1,    8, 16, 13, 8, 90,   0, 5,   -1, 16 },
                                                  new int[] { 160, 5, -1, 0, -1,   -1, -1, -1, -1,    8, 17, 14, 7, 90,   0, 5,   -1, 12 },
                                                  new int[] { 170, 5, -1, 0, -1,   -1, -1, -1, -1,    9, 19, 16, 7, 90,   0, 5,   -1,  8 }, };

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning - TODO
        public static int[][] resEcoLow = { new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    6, 19, 15, 8,  91,   0, 1,   -1, 25 },
                                            new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    6, 21, 17, 8,  98,   0, 1,   -1, 22 },
                                            new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    7, 23, 19, 7, 105,   0, 1,   -1, 18 },
                                            new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    8, 25, 21, 6, 112,   0, 1,   -1, 14 },
                                            new int[] { 2000, 50, -1, 0, -1,   -1, -1, -1, -1,    8, 28, 24, 6, 119,   0, 1,   -1, 10 }, };

        public static int[][] resEcoHigh = { new int[] { 150, 5, -1, 0, -1,   -1, -1, -1, -1,    6, 14, 12, 7, 64,   0, 3,   -1, 20 },
                                             new int[] { 155, 5, -1, 0, -1,   -1, -1, -1, -1,    6, 16, 14, 6, 69,   0, 3,   -1, 15 },
                                             new int[] { 160, 5, -1, 0, -1,   -1, -1, -1, -1,    6, 18, 16, 6, 73,   0, 3,   -1, 12 },
                                             new int[] { 165, 5, -1, 0, -1,   -1, -1, -1, -1,    7, 20, 18, 5, 78,   0, 3,   -1,  9 },
                                             new int[] { 170, 5, -1, 0, -1,   -1, -1, -1, -1,    8, 22, 20, 5, 83,   0, 3,   -1,  6 }, };

        // High floor levels to help with maintaining a single story.
        public static int[][] commercialLow = { new int[] { 100, 6, 1, 0,  90,   70, 20, 10,  0,    9, 30, 30, 9, 700,   0, 100,   -1, 30 },
                                                new int[] { 105, 6, 1, 0, 100,   30, 45, 20,  5,   10, 35, 35, 8, 750,   0,  90,   -1, 20 },
                                                new int[] { 110, 6, 1, 0, 110,    5, 30, 55, 10,   11, 40, 40, 7, 800,   0,  75,   -1, 10 }, };

        public static int[][] commercialHigh = { new int[] { 115, 5, 1, 0, 220,   10, 45, 40,  5,   10, 28, 28, 9, 750,   0, 80,   -1, 20 },
                                                 new int[] { 120, 5, 1, 0, 310,    7, 32, 43, 18,   11, 32, 32, 8, 800,   0, 70,   -1, 14 },
                                                 new int[] { 125, 5, 1, 0, 400,    5, 25, 45, 25,   13, 36, 36, 7, 850,   0, 60,   -1,  8 }, };

        public static int[][] commercialEco = { new int[] { 120, 6, 1, 0, 100,   50, 40, 10,  0,   11, 30, 30, 7, 800,   0,  2,   50, 20 } };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] { 1000, 10, 50, 0, 250, 15, 35, 35, 15, 30, 50, 55, 30, 900, 0, 150,  -1, 50 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] { 60, 10, 0, 0, 250, 15, 40, 35, 10, 30, 36, 40, 25, 750, 0, 300, -1, 30 } };

        public static int[][] office = { new int[] { 34, 5, 0, 0, -1,   2,  8, 20, 70,   12, 4, 4, 3, 1000,   0, 1,   10, 25 },
                                         new int[] { 36, 5, 0, 0, -1,   1,  5, 14, 80,   13, 5, 5, 3, 1125,   0, 1,   10, 37 },
                                         new int[] { 38, 5, 0, 0, -1,   1,  3,  6, 90,   14, 5, 5, 2, 1250,   0, 1,   10, 50 }, };

        public static int[][] officeHighTech = { new int[] { 74, 5, 0, 0, -1,   1,  2,  3, 94,   22, 5, 5, 3, 4000,   0, 1,   10, 10 } };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int[] { 38, 50, 0, 0, -1,   70, 20, 10,  0,   28,  90, 100, 20, 220,   300, 300,   100, 10 },
                                           new int[] { 35, 50, 0, 0, -1,   20, 45, 25, 10,   30, 100, 110, 18, 235,   150, 150,   140, 37 },
                                           new int[] { 32, 50, 0, 0, -1,    5, 20, 45, 30,   32, 110, 120, 16, 250,    25,  50,   160, 50 }, };

        public static int[][] industry_farm = { new int[] { 250, 50, 0, 0, -1,   90, 10,  0, 0,   10,  80, 100, 20, 180,   0, 175,    50, 10 },
                                                new int[] {  55, 25, 0, 0, -1,   30, 60, 10, 0,   40, 100, 150, 25, 220,   0, 180,   100, 25 }, };

        // The bounding box for a forest plantation is small
        public static int[][] industry_forest = { new int[] { 160, 50, 0, 0, -1,   90, 10,  0, 0,   20, 25, 35, 20, 180,   0, 210,    50, 10 },
                                                  new int[] {  45, 20, 0, 0, -1,   30, 60, 10, 0,   60, 70, 80, 30, 240,   0, 200,   100, 25 }, };

        public static int[][] industry_ore = { new int[] { 80, 50, 0, 0, -1,   18, 60, 20,  2,    50, 100, 100, 50, 250,   400, 500,    75, 10 },
                                               new int[] { 40, 30, 0, 0, -1,   15, 40, 35, 10,   120, 160, 170, 40, 320,   300, 475,   100, 25 }, };

        public static int[][] industry_oil = { new int[] { 80, 50, 0, 0, -1,   15, 60, 23,  2,    90, 180, 220, 40, 300,   450, 375,    75, 10 },
                                               new int[] { 38, 30, 0, 0, -1,   10, 35, 45, 10,   180, 200, 240, 50, 400,   300, 400,   100, 25 }, };

        // Bonus house hold data structure
        public static bool printResidentialNames = false;
        public static bool mergeResidentialNames = true;
        public static Dictionary<string, int> householdCache = new Dictionary<string, int>();
        public static Dictionary<string, int> bonusHouseholdCache = new Dictionary<string, int>();
        public static Dictionary<string, int> housePrintOutCache = new Dictionary<string, int>();
        public static Dictionary<string, int> defaultHousehold = new Dictionary<string, int>()
        {/*
            { "413694648.Tranquil Turquoise_Data", 36 },
            { "415635897.Highcliff from hongkong_Data", 92 },
            { "417797417.ZED68-PHOENIXTOWER-4x4HRL5_Data", 60 },
            { "421547247.Gula's Adam Highrise_Data", 55 },
            { "425922383.Pearl Tower 1.4_Data", 42 },
            { "453820359.Sprawl_Data", 74 },
            { "460321524.Truancy_Data", 66 },
            { "472267886.ZED68-PACHATOWER-4x4HRL5_Data", 69 },
            { "476362433.Yoshi Towers_Data", 80 },
            { "501398369.ZED68-HOPPER TOWER-4x4HRL5_Data", 70 },
            { "505937342.The Simian Tower_Data", 68 },
            { "572483919.Phitsanulok_Data", 93 },
            { "643034042.ZED68-OzOne Building (Ozo)_Data", 70 },
            { "665177868.PURGIO S_CITY 101_Data", 150 },
            { "666347361.Nurture_Data", 42 },
            { "672786901.jbr_rico_Data", 104 },
            { "673778168.PURGIO S_CITY 103_Data", 150 },
            { "677288436.The Rotterdam_Data", 400 },
            { "681918587.Marina Torch_Data", 400 },
            { "690181725.Aura Tower_Data", 210 },
            { "691942109.Sulafa tower_Data", 480 },
            { "700349615.The Pentominium_Data", 500 },
            { "719510655.Launch_Data", 240 },
            { "747968092.zenith_Data", 300 },
            { "749570687.Chelsea Tower_Data", 120 },
            { "754279432.Ahmed Abdul Rahim Al Attar Tower_Data", 272 },
            { "768450784.Damac residenze_Data", 300 },
            { "780452440.Haven_Data", 10 },
            { "781950755.Vozrojdenie_Data", 48 },
            { "792304093.Legenda_Data", 40 },
            { "793247873.Regatta_Data", 54 },
            { "789974462.Moskovskiy 14 floor 2 sec_Data", 28 },
            { "798976213.Evropeyskiy_Data", 75 },
            { "801913109.Arcadia_Data", 600 },
            { "803307441.Moskovskiy16fl2s_Data", 38 },
            { "815656717.Forte Tower 1_Data", 300 },
            { "815657350.Forte Tower 2_Data", 200 },
            { "817127292.Il Primo_Data", 340 },
            { "817127683.Il Primo 2_Data", 300 },
            { "819135593.Hive Cluster_Data", 400 },
            { "819890305.Burj Vista2_Data", 120 },
            { "819891420.Burj Vista_Data", 200 },
            { "823355275.H5 4x4 Platinum_Data", 100 },
            { "827270234.Emerald Park Condominium_Data", 100 },
            { "830090634.Millennium tower_Data", 200 },
            { "834378690.Juma Al Majid Tower_Data", 160 },
            { "840766570.Address Residence tower 1_Data", 240 },
            { "840814284.Address Residence tower 2_Data", 220 },
            { "841337944.Residential Building 011_Data", 120 },
            { "843120009.Modern Low-Rise Living #1_Data", 8 },
            { "850573663.Harmony 2_Data", 90 },
            { "850575631.Trident 4_Data", 93 },
            { "883950130.Aspect_Data", 92 },
            { "908476663.Opera Grand_Data", 280 },
            { "919075366.Forest_Data", 160 },
            { "948799048.Phitsanulok XL_Data", 160 },
            { "1099320836.Launch Prototype_Data", 220 },
            { "1138814303.Karlatornet_Data", 160 },
            { "1185669468.Darco_Data", 80 },
            { "1397694718.432 Park Avenue - New York_Data", 250 },
            { "1405018848.Nordstrom - Central Park Tower_Data", 260 },
            { "H4 4x3 Tenement10", 33 },
            { "H4 4x4 Tenement12", 36 },
            { "H5 4x4 Highrise02", 56 },
            { "H5 4x4 Highrise07", 70 },
            { "H5 4x4 Highrise08", 70 },
            { "L1 2x3 Detached05", 2 },
            { "L1 3x3 Detached02", 2 },
            { "L1 4x4 Detached02", 2 },
            { "L1 4x4 Detached06a", 2 },
            { "L1 4x4 Detached11", 2 },
            { "L2 2x2 Detached05", 2 },
            { "L2 2x3 Semi-detachedhouse01", 2 },
            { "L2 3x4 Semi-detachedhouse02a", 2 },
            { "L3 3x3 Semi-detachedhouse02", 2 },
            { "L3 4x4 Semi-detachedhouse03a", 2 }*/
        }; // end bonus households

        public static bool printEmploymentNames = false;
        public static bool mergeEmploymentNames = true;
        public static Dictionary<string, int> workerCache = new Dictionary<string, int>();
        public static Dictionary<string, int> bonusWorkerCache = new Dictionary<string, int>();
        public static Dictionary<string, int> workerPrintOutCache = new Dictionary<string, int>();
        public static Dictionary<string, int> defaultWorker = new Dictionary<string, int>()
        {/*
            { "419078725.Barry Plaza_Data", 500 },
            { "422434231.Bank of America_Data", 520 },
            { "422472215.Lever Tower_Data", 180 },
            { "426607732.Ares Tower_Data", 5000 },
            { "428001234.Trump Tower Chicago_Data", 2800 },
            { "428925673.Leviathan_Data", 1000 },
            { "432230772.Antares Tower_Data", 2800 },
            { "434097849.Monolith_Data", 500 },
            { "435267158.Twin Monoliths_Data", 1000 },
            { "439107462.Gula's Rivergate Lykes_Data", 360 },
            { "453154792.Gula's Kingdom Tower_Data", 6500 },
            { "476346019.Bluerise Plaza_Data", 1200 },
            { "477861822.Sagan Tyson Center for Space_Data", 100 },
            { "507302735.Banbury_Data", 380 },
            { "509112645.Crystallization_Data", 700 },
            { "516614116.Taipei 101_Data", 3500 },
            { "522207137.REWE supermarket_Data", 30 },
            { "527535273.snowjaoONEONE_Data", 3400 },
            { "529294835.REAL supermarket_Data", 40 },
            { "531215484.Shanghai Tower, Shanghai (1:1)_Data", 10000 },
            { "534117358.Two Prudential Plaza_Data", 2600 },
            { "534142273.TorreTrump_World_Tower_Data", 1200 },
            { "548854239.ZED68-HALFCHASE-4x4OFL3_Data", 300 },
            { "557430842.Columbia Center_Data", 3050 },
            { "568336474.Javelin_Data", 340 },
            { "579129058.Figueroa at Wilshire 1:1_Data", 2500 },
            { "591074989.The Tower_Data", 1200 },
            { "604009267.One World Trade Center 1:1_Data", 8500 },  // 325279 sqm
            { "605000091.60 Wall Street_Data", 2700 },
            { "607981950.Chirpigroup Center_Data", 1600 },
            { "615095287.GITS Huge Office Building(ID#04)_Data", 14000 },
            { "621431766.Glory_Data", 1600 },
            { "622221778.Stalwart_Data", 430 },
            { "626845833.The Empire State Building_Data", 5400 },  // 208879 sqm
            { "628101617.Pan Pacific Insurance_Data", 1600 },
            { "640403421.GITS Office Tower (ID#02c)_Data", 1500 },
            { "647059381.GiTS Mega Office Bldg (ID#05)_Data", 20000 },
            { "651651763.Alliance_Data", 220 },
            { "652765729.Zed68-Mint 3x4L3Off_Data", 240 },
            { "660411166.Elstree Building_Data", 1400 },
            { "660864619.Policy_Data", 245 },
            { "661075324.Santander Tower_Data", 1800 },
            { "661823191.Whirligig XL_Data", 1100 },
            { "670147764.Old brick warehouse_Data", 10 },
            { "670944160.Big Office Building_Data", 500 },
            { "672448363.Foster Tower Madrid (Red)_Data", 1200 },
            { "672525995.Foster Tower Madrid (White)_Data", 1200 },
            { "676602837.PWC Tower Madrid_Data", 2000 },
            { "678159892.Crystal Tower Madrid_Data", 2000 },
            { "680500415.Space Tower Madrid_Data", 1800 },
            { "687777086.Mirage_Data", 1400 },
            { "694499934.Old factory 8x6_Data", 25 },
            { "694598864.Old factory 8x6 - Concrete_Data", 25 },
            { "700549442.8 Canada Square_Data", 2400 },
            { "700553372.One Canada Square_Data", 2900 },
            { "708476856.Al habtoor business tower_Data", 470 },
            { "708992454.Eclipse_Data", 2000 },
            { "717997754.Miyagi Motors front_Data", 100 },
            { "717956285.Miyagi Motors Main Office_Data", 200 },
            { "717963179.Miyagi Motors front wide roof_Data", 100 },
            { "717980167.Miyagi Motors back wide roof_Data", 100 },
            { "718003124.Miyagi Motors back_Data", 100 },
            { "718013084.Miyagi Motors front high roof_Data", 100 },
            { "718016993.Miyagi Motors back high roof_Data", 100 },
            { "718023336.Miyagi Motors back w smokestack_Data", 100 },
            { "718025758.Miyagi Motors front gable roof_Data", 100 },
            { "718029264.Miyagi Motors back gable roof_Data", 100 },
            { "729503348.Factory Hall 1_Data", 120 },
            { "730926381.The Bay Gate large_Data", 1800 },
            { "740080687.Factory Hall 2_Data", 120 },
            { "740085564.Warehouse 1_Data", 10 },
            { "743386704.Warehouse 2_Data", 5 },
            { "743387884.Warehouse 2 - colors_Data", 10 },
            { "744947488.Old warehouse 2 - red_Data", 5 },
            { "744948035.Old warehouse 2 - yellow_Data", 5 },
            { "749227212.Abeno Harukas_Data", 3600 },
            { "751581536.Research Facility_Data", 100 },
            { "751649166.Old warehouse 3 - Brick_Data", 15 },
            { "751649722.Old warehouse 3 - Concrete_Data", 15 },
            { "752845832.Warehouse 3_Data", 10 },
            { "754236528.Warehouse 4_Data", 15 },
            { "766038657.San'tElia Tower_Data", 2400 },
            { "762728624.Goldin Finance 117_Data", 8000 },
            { "777048160.Sunshine 60 RICO_Data", 2000 },
            { "777050916.Mustard Inc_Data", 100 },
            { "778876495.The Aon Center_Data", 4000 },
            { "779406148.McGinnis Tower_Data", 2000 },
            { "784540127.Distribution Center FedEx_Data", 20 },
            { "782848688.Distribution Center Long_Data", 100 },
            { "785268004.FIB Los Sanots HQ _Data", 1400 },
            { "786448278.Cancer Research Center_Data", 140 },
            { "792727102.Quadrillion_Data", 10000 },
            { "794601473.Woodlog Architects_Data", 100 },
            { "796614604.Perth Council Building_Data", 100 },
            { "796870246.Ivory Dream_Data", 6000 },
            { "798838680.Commerce Court Complex_Data", 1200 },
            { "799186937.Perth CitiBank House_Data", 210 },
            { "799801877.100 St Georges Terrace_Data", 540 },
            { "800259881.700 De La Gauchetiere_Data", 1700 },
            { "800259881.National Bank Tower_Data", 800 },
            { "800260798.Place Ville Marie_Data", 2000 },
            { "804749788.Event Hall_Data", 50 },
            { "810925693.Cosine Building_Data", 220 },
            { "815068649.Triangle of Siam_Data", 500 },
            { "818407172.Community Center_Data", 140 },
            { "834382288.Freedom of Expression_Data", 2000 },
            { "844946740.Amethyst_Data", 120 },
            { "850569287.Address BLVD_Data", 500 },
            { "853783969.Gran Torre Santiago_Data", 3200 },
            { "858234831.One Liberty Plaza 1:1_Data", 2000 },
            { "865034481.One World Trade Center_Data", 8500 },  // 325279 sqm
            { "869593360.Concomitant_Data", 400 },
            { "873468408.World Port Center_Data", 460 },
            { "928132734.Liberty Tower_Data", 160 },
            { "935498035.State_Data", 375 },
            { "941007962.Alliance XL_Data", 340 },
            { "945453381.Virgo XL_Data", 200 },
            { "969613932.Conundrum XL_Data", 100 },
            { "1132988810.Proprietary_Data", 2800 },
            { "1208800478.Small office 01_Data", 10 },
            { "1206400730.JW Marriott Marquis_Data", 300 },
            { "1241535932.United Building_Data", 1800 },
            { "1266399912.Coda_Data", 3400 },
            { "1231052548.Luminasoft_Data", 1900 },
            { "1238393229.Haywire_Data", 3000 },
            { "1288986198.Fractal_Data", 2500 },
            { "1466001439.Australia Square tower_Data", 1000 },
            { "1470018515.Rama IX Super Tower_Data", 11000 },
            { "1495622576.The Center Remastered_Data", 3600 },
            { "H2 4x3 BigFactory08", 20 }*/
        };

        // Prefab stores
        // public static Dictionary<int, int> prefabHouseHolds = new Dictionary<int, int>(512);
        // public static Dictionary<int, PrefabEmployStruct> prefabWorkerVisit = new Dictionary<int, PrefabEmployStruct>(1024);
        public static Dictionary<ulong, ushort> seedToId = new Dictionary<ulong, ushort>();

        public static void ClearCache()
        {
            workerCache.Clear();
            householdCache.Clear();
            bonusWorkerCache.Clear();
            bonusHouseholdCache.Clear();
            workerPrintOutCache.Clear();
            housePrintOutCache.Clear();

            // prefabWorkerVisit.Clear();
            // prefabHouseHolds.Clear();
            seedToId.Clear();

            printResidentialNames = false;
            printEmploymentNames = false;
            mergeResidentialNames = true;
            mergeEmploymentNames = true;

            strictCapacity = true;
            allowRemovalOfCitizens = false;
        }

        /// <summary>
        /// Struct for caching building's consumption values.
        /// </summary>
        public struct ConsumeStruct
        {
            public int productionRate;
            public int electricity;
            public int water;
            public int sewage;
            public int garbage;
            public int income;
        }
    } // end DataStore
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
