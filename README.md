# NamecheapDynDNS
A .NET Namecheap Dynamic DNS client that runs in the background as a Windows service.

# Namecheap Setup
1. Log into your namecheap.com account.
2. Click the "Manage" button for your domain.
3. Click the "Advanced DNS" tab.
4. Scroll down to the "Dynamic DNS" section.
5. Turn on the "Status" toggle to enable Dynamic DNS for your domain and to generate your Dynamic DNS password.
6. Add 1 or more Dynamic DNS hosts for your domain.

# Installation
1. Install .NET Framework 4.7 SDK
2. Build the project.
3. Copy the files from the project's bin folder to your preferred install directory.
4. Open the Visual Studio Developer Command Prompt and go to your chosen install directory.
5. Type: installutil NamecheapDynDNS.exe
6. Update the domain-config.json file with your Namecheap Dynamic DNS settings.