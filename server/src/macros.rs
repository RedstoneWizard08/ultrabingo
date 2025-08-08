#[macro_export]
macro_rules! level {
    ($name: expr, $scene: expr) => {
        $crate::pools::Level {
            scene_name: $scene,
            name: $name,
            is_angry: false,
            angry_parent_bundle: "",
        }
    };

    ($name: expr, $scene: expr, $bundle: expr) => {
        $crate::pools::Level {
            scene_name: $scene,
            name: $name,
            is_angry: true,
            angry_parent_bundle: $bundle,
        }
    };
}

#[macro_export]
macro_rules! pool {
    {
        id = $id: ident,
        name = $name: expr,
        desc = $desc: expr,
        maps = [$($level: expr => $scene: expr,)+]$(,)?
    } => {
        pub const $id: $crate::pools::MapPool = $crate::pools::MapPool {
            name: $name,
            description: $desc,
            maps: &[
                $(
                    $crate::pools::Level {
                        scene_name: $scene,
                        name: $level,
                        is_angry: false,
                        angry_parent_bundle: "",
                    },
                )+
            ],
        };
    };

    {
        id = $id: ident,
        name = $name: expr,
        desc = $desc: expr,
        maps = [$($level: expr => $scene: expr => $bundle: expr,)+]$(,)?
    } => {
        pub const $id: $crate::pools::MapPool = $crate::pools::MapPool {
            name: $name,
            description: $desc,
            maps: &[
                $(
                    $crate::pools::Level {
                        scene_name: $scene,
                        name: $level,
                        is_angry: true,
                        angry_parent_bundle: $bundle,
                    },
                )+
            ],
        };
    };
}

#[macro_export]
macro_rules! pools {
    [
        $(
            {
                id = $id: ident,
                key = $key: expr,
                name = $name: expr,
                desc = $desc: expr,
                maps = [$($level: expr => $scene: expr,)+]$(,)?
            },
        )+

        $(
            angry {
                id = $a_id: ident,
                key = $a_key: expr,
                name = $a_name: expr,
                desc = $a_desc: expr,
                maps = [$($a_level: expr => $a_scene: expr => $a_bundle: expr,)+]$(,)?
            },
        )*
    ] => {
        $(pub const $id: $crate::pools::MapPool = $crate::pools::MapPool {
            name: $name,
            description: $desc,
            maps: &[
                $(
                    $crate::pools::Level {
                        scene_name: $scene,
                        name: $level,
                        is_angry: false,
                        angry_parent_bundle: "",
                    },
                )+
            ],
        };)+

        $(pub const $a_id: $crate::pools::MapPool = $crate::pools::MapPool {
            name: $a_name,
            description: $a_desc,
            maps: &[
                $(
                    $crate::pools::Level {
                        scene_name: $a_scene,
                        name: $a_level,
                        is_angry: true,
                        angry_parent_bundle: $a_bundle,
                    },
                )+
            ],
        };)*

        pub const MAP_POOLS: $crate::phf::OrderedMap<&'static str, $crate::pools::MapPool> = $crate::phf::phf_ordered_map! {
            $($key => $id,)+
            $($a_key => $a_id,)*
        };
    };
}
