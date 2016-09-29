# Downloads a news RSS feed and reads out the headlines in morse code.
SOURCE=http://feeds.reuters.com/Reuters/worldNews?format=xml
curl $SOURCE 2>/dev/null \
    | sed 's/></>\n</g' \
    | grep '<title>' \
    | sed  's/<title>\(.*\)<\/title>/\1/g' \
    | sed "s/&apos;/'/g" \
    | sed 's/Top Stories - Google News//g' \
    | sed 's/Reuters News//g' \
    | sed 's/Reuters: World News//g' \
    | ipython morse_player.py -- -v 20 