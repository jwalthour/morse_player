# Downloads a news RSS feed and reads out the headlines in morse code.
function readNews {
    SOURCE=$1
    echo Downloading $SOURCE
    curl $SOURCE 2>/dev/null \
        | sed 's/></>\n</g' \
        | grep '<title>' \
        | sed  's/<title>\(.*\)<\/title>/\1/g' \
        | sed "s/&apos;/'/g" \
        | sed 's/Top Stories - Google News//g' \
        | sed 's/Reuters News//g' \
        | sed 's/Reuters: World News//g' \
        | ipython morse_player.py -- -v 20;
}
SOURCE0=http://feeds.reuters.com/Reuters/worldNews?format=xml
SOURCE1=https://news.google.com/?output=rss
while [ 1 ]; do
    readNews $SOURCE0
    readNews $SOURCE1
done
