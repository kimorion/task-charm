#!/bin/bash
git pull
sudo killall ngrok
nohup ngrok http 8444 &
sleep 2
url=$(curl localhost:4040/api/tunnels | grep -o "https[^\"]*ngrok.io")
echo got new url: "$url"
echo
sed -i 's,https[^"]*ngrok.io,'"$url"',' ./Charm.Application/conf/telegram-bot-settings.json
echo updated config:
cat ./Charm.Application/conf/telegram-bot-settings.json
echo
echo starting Charm...
sudo docker-compose up -d
