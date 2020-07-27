#/bin/sh

/home/example/BytemarkDNS/target/BytemarkDNS create domain --name "_acme-challenge.$CERTBOT_DOMAIN" --type TXT --ttl 30 --content "$CERTBOT_VALIDATION"
