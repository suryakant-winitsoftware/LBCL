"use client";

import * as React from "react";
import { Check, ChevronDown, Phone } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Input } from "@/components/ui/input";
import { ScrollArea } from "@/components/ui/scroll-area";

// Country data with codes, flags, and phone formats
const countries = [
  {
    code: "US",
    name: "United States",
    dialCode: "+1",
    flag: "🇺🇸",
    format: "(000) 000-0000",
    maxLength: 10,
  },
  {
    code: "GB",
    name: "United Kingdom",
    dialCode: "+44",
    flag: "🇬🇧",
    format: "0000 000000",
    maxLength: 10,
  },
  {
    code: "IN",
    name: "India",
    dialCode: "+91",
    flag: "🇮🇳",
    format: "00000 00000",
    maxLength: 10,
  },
  {
    code: "CA",
    name: "Canada",
    dialCode: "+1",
    flag: "🇨🇦",
    format: "(000) 000-0000",
    maxLength: 10,
  },
  {
    code: "AU",
    name: "Australia",
    dialCode: "+61",
    flag: "🇦🇺",
    format: "000 000 000",
    maxLength: 9,
  },
  {
    code: "DE",
    name: "Germany",
    dialCode: "+49",
    flag: "🇩🇪",
    format: "0000 0000000",
    maxLength: 11,
  },
  {
    code: "FR",
    name: "France",
    dialCode: "+33",
    flag: "🇫🇷",
    format: "0 00 00 00 00",
    maxLength: 9,
  },
  {
    code: "IT",
    name: "Italy",
    dialCode: "+39",
    flag: "🇮🇹",
    format: "000 000 0000",
    maxLength: 10,
  },
  {
    code: "ES",
    name: "Spain",
    dialCode: "+34",
    flag: "🇪🇸",
    format: "000 00 00 00",
    maxLength: 9,
  },
  {
    code: "JP",
    name: "Japan",
    dialCode: "+81",
    flag: "🇯🇵",
    format: "00-0000-0000",
    maxLength: 10,
  },
  {
    code: "CN",
    name: "China",
    dialCode: "+86",
    flag: "🇨🇳",
    format: "000 0000 0000",
    maxLength: 11,
  },
  {
    code: "BR",
    name: "Brazil",
    dialCode: "+55",
    flag: "🇧🇷",
    format: "(00) 00000-0000",
    maxLength: 11,
  },
  {
    code: "MX",
    name: "Mexico",
    dialCode: "+52",
    flag: "🇲🇽",
    format: "00 0000 0000",
    maxLength: 10,
  },
  {
    code: "RU",
    name: "Russia",
    dialCode: "+7",
    flag: "🇷🇺",
    format: "(000) 000-00-00",
    maxLength: 10,
  },
  {
    code: "ZA",
    name: "South Africa",
    dialCode: "+27",
    flag: "🇿🇦",
    format: "00 000 0000",
    maxLength: 9,
  },
  {
    code: "KR",
    name: "South Korea",
    dialCode: "+82",
    flag: "🇰🇷",
    format: "00-0000-0000",
    maxLength: 10,
  },
  {
    code: "AE",
    name: "UAE",
    dialCode: "+971",
    flag: "🇦🇪",
    format: "00 000 0000",
    maxLength: 9,
  },
  {
    code: "SA",
    name: "Saudi Arabia",
    dialCode: "+966",
    flag: "🇸🇦",
    format: "00 000 0000",
    maxLength: 9,
  },
  {
    code: "SG",
    name: "Singapore",
    dialCode: "+65",
    flag: "🇸🇬",
    format: "0000 0000",
    maxLength: 8,
  },
  {
    code: "NZ",
    name: "New Zealand",
    dialCode: "+64",
    flag: "🇳🇿",
    format: "00 000 0000",
    maxLength: 9,
  },
  {
    code: "ID",
    name: "Indonesia",
    dialCode: "+62",
    flag: "🇮🇩",
    format: "000-0000-0000",
    maxLength: 11,
  },
  {
    code: "PH",
    name: "Philippines",
    dialCode: "+63",
    flag: "🇵🇭",
    format: "000 000 0000",
    maxLength: 10,
  },
  {
    code: "TH",
    name: "Thailand",
    dialCode: "+66",
    flag: "🇹🇭",
    format: "00 000 0000",
    maxLength: 9,
  },
  {
    code: "MY",
    name: "Malaysia",
    dialCode: "+60",
    flag: "🇲🇾",
    format: "00-000 0000",
    maxLength: 10,
  },
  {
    code: "VN",
    name: "Vietnam",
    dialCode: "+84",
    flag: "🇻🇳",
    format: "00 0000 0000",
    maxLength: 10,
  },
  {
    code: "PK",
    name: "Pakistan",
    dialCode: "+92",
    flag: "🇵🇰",
    format: "000 0000000",
    maxLength: 10,
  },
  {
    code: "BD",
    name: "Bangladesh",
    dialCode: "+880",
    flag: "🇧🇩",
    format: "0000-000000",
    maxLength: 10,
  },
  {
    code: "NG",
    name: "Nigeria",
    dialCode: "+234",
    flag: "🇳🇬",
    format: "000 000 0000",
    maxLength: 10,
  },
  {
    code: "EG",
    name: "Egypt",
    dialCode: "+20",
    flag: "🇪🇬",
    format: "00 0000 0000",
    maxLength: 10,
  },
  {
    code: "KE",
    name: "Kenya",
    dialCode: "+254",
    flag: "🇰🇪",
    format: "000 000000",
    maxLength: 9,
  },
].sort((a, b) => a.name.localeCompare(b.name));

interface PhoneInputProps {
  value?: string;
  onChange?: (value: string) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  defaultCountry?: string;
  required?: boolean;
}

export function PhoneInput({
  value = "",
  onChange,
  placeholder = "Enter phone number",
  disabled = false,
  className,
  defaultCountry = "US",
  required = false,
}: PhoneInputProps) {
  const [open, setOpen] = React.useState(false);
  const [selectedCountry, setSelectedCountry] = React.useState(
    countries.find((c) => c.code === defaultCountry) || countries[0]
  );
  const [phoneNumber, setPhoneNumber] = React.useState("");
  const [searchTerm, setSearchTerm] = React.useState("");

  // Parse existing value
  React.useEffect(() => {
    if (value) {
      // Check if value starts with a dial code
      const matchedCountry = countries.find((c) =>
        value.startsWith(c.dialCode)
      );
      if (matchedCountry) {
        setSelectedCountry(matchedCountry);
        setPhoneNumber(value.replace(matchedCountry.dialCode, "").trim());
      } else {
        // If no dial code, just set the number
        setPhoneNumber(value);
      }
    }
  }, [value]);

  // Format phone number based on country format
  const formatPhoneNumber = (input: string, country: (typeof countries)[0]) => {
    // Remove all non-digit characters
    const digits = input.replace(/\D/g, "");

    // Limit to max length
    const limited = digits.slice(0, country.maxLength);

    // Apply format
    let formatted = "";
    let digitIndex = 0;

    for (
      let i = 0;
      i < country.format.length && digitIndex < limited.length;
      i++
    ) {
      if (country.format[i] === "0") {
        formatted += limited[digitIndex];
        digitIndex++;
      } else if (digitIndex > 0 || country.format[i] === "(") {
        formatted += country.format[i];
      }
    }

    return formatted;
  };

  const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const input = e.target.value;
    const formatted = formatPhoneNumber(input, selectedCountry);
    setPhoneNumber(formatted);

    // Call onChange with full number including country code
    if (onChange) {
      const cleanNumber = formatted.replace(/\D/g, "");
      onChange(cleanNumber ? `${selectedCountry.dialCode} ${formatted}` : "");
    }
  };

  const handleCountryChange = (countryCode: string) => {
    const country = countries.find((c) => c.code === countryCode);
    if (country) {
      setSelectedCountry(country);
      setOpen(false);
      setSearchTerm("");

      // Reformat existing number with new country format
      if (phoneNumber) {
        const digits = phoneNumber.replace(/\D/g, "");
        const formatted = formatPhoneNumber(digits, country);
        setPhoneNumber(formatted);

        if (onChange) {
          onChange(formatted ? `${country.dialCode} ${formatted}` : "");
        }
      }
    }
  };

  const filteredCountries = countries.filter(
    (country) =>
      searchTerm === "" ||
      country.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      country.dialCode.includes(searchTerm) ||
      country.code.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className={cn("flex gap-2", className)}>
      {/* Country Code Selector */}
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger asChild>
          <Button
            variant="outline"
            role="combobox"
            aria-expanded={open}
            className="w-[140px] justify-between h-10"
            disabled={disabled}
          >
            <span className="flex items-center gap-2">
              <span className="text-lg">{selectedCountry.flag}</span>
              <span className="text-sm font-medium">
                {selectedCountry.dialCode}
              </span>
            </span>
            <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-[300px] p-0">
          <Command>
            <CommandInput
              placeholder="Search country..."
              value={searchTerm}
              onValueChange={setSearchTerm}
            />
            <CommandEmpty>No country found.</CommandEmpty>
            <CommandGroup>
              <ScrollArea className="h-[300px]">
                {filteredCountries.map((country) => (
                  <CommandItem
                    key={country.code}
                    value={country.code}
                    onSelect={() => handleCountryChange(country.code)}
                    className="flex items-center justify-between cursor-pointer"
                  >
                    <div className="flex items-center gap-2">
                      <span className="text-lg">{country.flag}</span>
                      <span className="font-medium">{country.name}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="text-sm text-muted-foreground">
                        {country.dialCode}
                      </span>
                      <Check
                        className={cn(
                          "h-4 w-4",
                          selectedCountry.code === country.code
                            ? "opacity-100"
                            : "opacity-0"
                        )}
                      />
                    </div>
                  </CommandItem>
                ))}
              </ScrollArea>
            </CommandGroup>
          </Command>
        </PopoverContent>
      </Popover>

      {/* Phone Number Input */}
      <div className="relative flex-1">
        <Phone className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
        <Input
          type="tel"
          value={phoneNumber}
          onChange={handlePhoneChange}
          placeholder={placeholder || selectedCountry.format.replace(/0/g, "•")}
          disabled={disabled}
          required={required}
          className="pl-10 h-10"
        />
      </div>
    </div>
  );
}

// Export countries for external use if needed
export { countries };
