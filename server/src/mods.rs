use crate::info::DEV_IDS;

pub const WHITELISTED_MODS: &[&str] = &[
    "AngryLevelLoader",
    "Baphomet's BINGO",
    "BetterWeaponHUDs",
    "Configgy",
    "Damage Style HUD",
    "EasyPZ",
    "HandPaint",
    "Healthbars",
    "JadeLib",
    "IntroSkip",
    "PluginConfigurator",
    "StyleEditor",
    "StyleToasts",
    "USTManager",
];

pub fn check_mods(steam_id: impl AsRef<str>, installed: Vec<String>) -> Vec<String> {
    if DEV_IDS.contains(&steam_id.as_ref()) {
        return vec![];
    }

    let mut unauthorized = Vec::new();

    for item in installed {
        if !WHITELISTED_MODS.contains(&item.as_str()) {
            unauthorized.push(item);
        }
    }

    if !unauthorized.is_empty() {
        warn!(
            "Client is using non-whitelisted mods: {}",
            unauthorized.join(", ")
        );
    }

    unauthorized
}
