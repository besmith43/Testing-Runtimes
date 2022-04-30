use mac_address::mac_address_by_name;
use gethostname::gethostname;
use sys_info::os_type;
use csv::Writer;
use chrono::prelude::*;
use std::error::Error;

fn save_csv(csv_file: &str, host: &str, address: &str, os: &str) -> Result<(), Box<dyn Error>> {
    let mut wtr = Writer::from_path(csv_file)?;
    wtr.write_record(&["hostname", "mac address", "operating system"])?;
    wtr.write_record(&[host, address, os])?;
    wtr.flush()?;
    Ok(())
}

fn main() {
    let hostname = gethostname().into_string().unwrap();

    let os = os_type().unwrap();

    let today = Local::now();

    let csv_name = format!("{}{}{}-{}.csv", today.month(), today.day(), today.year(), hostname);

    #[cfg(any(target_os = "linux"))]
    let name = "eth0";

    #[cfg(any(target_os = "macos"))]
    let name = "en0";

    #[cfg(any(target_os = "freebsd"))]
    let name = "em0";

    #[cfg(target_os = "windows")]
    let name = "Ethernet";

    match mac_address_by_name(name) {
        Ok(Some(ma)) => {
            save_csv(&csv_name, &hostname, &(format!("{}", ma)), &os).unwrap();
        }
        Ok(None) => println!("Interface \"{}\" not found", name),
        Err(e) => println!("{:?}", e),
    }
}
