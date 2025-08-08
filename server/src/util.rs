use chrono::TimeDelta;
use rand::{Rng, RngCore};
use regex::Regex;
use std::time::Duration;

pub const ONE_SECOND: Duration = Duration::from_secs(1);
pub const VOTE_TIMER_SECS: Duration = Duration::from_secs(25);
pub const CLIENT_VERSION: &str = "1.1.1";

pub fn sanitize_username(input: impl AsRef<str>) -> String {
    let emoticons = Regex::new(r"[\x{1F600}-\x{1F64F}]").unwrap();
    let symbols = Regex::new(r"[\x{1F300}-\x{1F5FF}]").unwrap();
    let transport = Regex::new(r"[\x{1F680}-\x{1F6FF}]").unwrap();
    let match_flags = Regex::new(r"[\x{1F1E0}-\x{1F1FF}]").unwrap();

    match_flags
        .replace(
            &transport.replace_all(
                &symbols.replace_all(&emoticons.replace_all(input.as_ref(), ""), ""),
                "",
            ),
            "",
        )
        .to_string()
}

pub fn random_element<T: Clone>(vec: &Vec<T>) -> T {
    vec[rand::rng().random_range(0..vec.len())].clone()
}

pub fn random_element_pop<T: Clone>(vec: &mut Vec<T>) -> T {
    let idx = rand::rng().random_range(0..vec.len());
    let el = vec[idx].clone();

    vec.remove(idx);
    el
}

pub fn generate_code() -> String {
    let mut data = [0u8; 3];

    rand::rng().fill_bytes(&mut data);

    data.into_iter().map(|it| format!("{:x}", it)).collect()
}

pub fn format_time_delta(delta: TimeDelta) -> String {
    let seconds = delta.num_seconds() % 60;
    let minutes = (delta.num_seconds() / 60) % 60;
    let hours = (delta.num_seconds() / 60) / 60;

    format!("{:0>2}:{:0>2}:{:0>2}", hours, minutes, seconds)
}
