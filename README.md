# NamecheapDynDNS
A .NET Namecheap Dynamic DNS client that runs in the background as a Windows service.

# Namecheap Setup
1. Log into your namecheap.com account.
1. Click the "Manage" button for your domain.
1. Click the "Advanced DNS" tab.
1. Scroll down to the "Dynamic DNS" section.
1. Turn on the "Status" toggle to enable Dynamic DNS for your domain and to generate your Dynamic DNS password.
1. Add 1 or more Dynamic DNS hosts for your domain.

# Installation
1. Install .NET 6.x SDK
1. Publish the project in Release mode to your preferred install directory.
1. Create the Windows Service:
    ```bash
    sc.exe create "Namecheap Dynamic DNS Client" binpath="C:\Path\To\NamecheapDynDNS.exe"
    ```
1. Update the appsettings.Production.json file with your Namecheap Dynamic DNS settings.