# downloads and speaks fortunes
BASE_URL="http://anduin.eldar.org/cgi-bin/fortune.pl?text_format=yes&fortune_db="
# DB_LIST="fortunes fortunes2 limerick pratchett startrek bofh-excuses futurama farber netbsd"
DB_LIST="fortunes fortunes2 pratchett startrek bofh-excuses futurama farber netbsd"

while true; do
	for db in $DB_LIST; do
		SOURCE="$BASE_URL$db"
		echo "Using $db"
		curl $SOURCE 2>/dev/null | cat | ipython morse_player.py 2>/dev/null
		# curl $SOURCE 
		# sleep 1
		sleep 10
	done
done
