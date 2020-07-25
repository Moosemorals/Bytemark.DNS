
# Bytemark DNS - Automation tool for the Bytemark Content DNS service

I have been a happy customer of [Bytemark](https://bytemark.co.uk) for more years than I care to
remember. Sometime in the last few years they've updated their 'content
DNS' (DNS hosting) service from a [TinyDNS](https://cr.yp.to/djbdns/tinydns.html)/rsync
based system to one using a JSON/REST web frontend.

Using dotnet core, and the [API documentation](https://dns.bytemark.co.uk/api/v1/doc/index.html)
I've put together a client, and used it to make wildcard certificates
from [Let's Encrypt](https://letsencrypt.org/).

## Licence

The code in this repository is covered by the [ISC Licence](LICENCE.txt). (Basicaly MIT but with
even less text).

## Howto

1. Install dependencies. This assumes Debian, other systems may vary. I also assume that you're
    fairly confident with Linux, and that you've used certbot before.

    1.1 Dotnet core - Follow Microsoft's instructions: https://docs.microsoft.com/en-us/dotnet/core/install/linux-debian

    1.2 Certbot - 

        ```
        # apt install certbot
        ```

2. Clone this repo:

    ```
    $ git clone https://github.com/moosemorals/BytemarkDNS.git
    ```

3. Create a secrets file with your Bytemark Panel userid and password

    ```
    $ cd BytemarkDNS
    $ cat > secrets.json
    {
	    "Auth": {
		    "Username": "exampleUser",
		    "Password": "correct horse battery staple"
	    }
    }
    ```

4. Build the dotnet project

   ```
   $  dotnet publish --output target --no-self-contained --nologo --configuration Release
   ```

5. Install the two shell scripts ([authhook.sh](scripts/authhook.sh) and [cleanup.sh](scripts/cleanup.sh))
   somewhere that root can run them.

   ```
   $ sudo mkdir -p /root/bin
   $ sudo cp authhook.sh cleanup.sh /root/bin
   ```

6. Change the paths in the scripts to point at the `target` directory from step 4.

7. Generate certificates (split onto two lines to make it easier to read, should only be one)

   ```
   $ sudo certbot certonly -d example.com -d "*.example.com" --manual --preferred-challenges=dns 
       --manual-auth-hook /root/bin/auth-hook.sh --manual-cleanup-hook /root/bin/cleanup.sh --manual-public-ip-logging-ok 
   ```

## How it works

The ACME dns challenge (which is needed for wildcard certs) works by having the domain owner
create a TXT record called '_acme-challenge' as a child of the domain to be verified, with
the contents of the record a random string.

My code asks the Bytemark API for a list of your domains, checks that you own the domain
you're asking for, creates the appropriate TXT record, and then cleans up after itself.

# Support

Any problems, please raise an issue, although this is a toy for me so please have low
expectations about speedy resolution.
