#Forex Factory Scraper
import cloudscraper
from bs4 import BeautifulSoup
from datetime import date, timedelta, datetime
import re
import time
import csv
import calendar
import sys
import os

list_of_news_events = []

# Used to hold event time as not all event times have a time if multiple news events start at the same time

#calendar?day=apr15.2024"
def getEventsCalendar(start, end, file_path):

	scraper = cloudscraper.create_scraper() # Need cloudscraper to bypass cloudflare

	start_date = f"{start.strftime('%b').lower()}{start.strftime('%d')}.{start.strftime('%Y')}"
	end_date = f"{end.strftime('%b').lower()}{end.strftime('%d')}.{end.strftime('%Y')}"
	# Gets One Day at a time
	#
	# print('start date: ' + start_date)
	# print('end date: ' + end_date + '\n')
	# specify the url
	url = f'https://www.forexfactory.com/calendar?day={start_date}'

	# query the website and return the html to the variable ‘page’
	page = scraper.get(url).text
	# print(page)
	# parse the html using beautiful soup and store in variable `soup`
	soup = BeautifulSoup(page, 'html.parser')
	# Take out the <div> of name and get its value

	#timezone = soup.findAll('a', class_='member-nav--angle member-nav--angle-ll')
	# Find the table containing all the data
	table = soup.find('table', class_ = 'calendar__table')

	# Date of Event
	date_of_events = table.find_next('tr', class_ = 'calendar__row--new-day').find_next('span', class_ = 'date')

	# Regualr Expression to find the 'day of week', 'month' and the 'day'
	matchObj = re.search('([a-zA-Z]{3}) ([a-zA-Z]{3}) ([0-9]{1,2})', date_of_events.text)

	# Assigning the 'day of week', 'month' and 'day'

	day_of_week = matchObj.group(1)
	month = matchObj.group(2)
	month = strToIntMonth(month) # Convert from Str to Int
	# if month in monthsList:
	# 	print(month)
	month = int(format(month, "02")) # Places 0's in front of the month if it is single digit day, for months Jan - Sep
	day = matchObj.group(3)
	day = int(format(int(day), "02")) # Places 0's in front of the day if it is single digit day, for days 1-9 of the month
	year = int(start_date[-4:])

	# Event Times 
	event_times = table.find_all('td', class_ = 'calendar__time')

	if(day_of_week != 'Sat' and day_of_week != 'Sun' ):
		event_time_holder = '' # Holds event time of previous news event if it does not have one
		event_time_hour = '0'
		event_time_minutes = ':00'
		am_or_pm = 'am'
		event_date_time = ''
		curr = ''
		for news in event_times:
			label = news.find_next_sibling('td', class_ = 'calendar__currency').text.strip()
			if(label != 'All'):
				curr = label
			impact = news.find_next_sibling('td', class_ = 'calendar__impact').find_next('span')['class']
			impact = impact[1][-3:]
			event = news.find_next_sibling('td', class_ = 'calendar__event').find_next('span').text.strip()
			previous = news.find_next_sibling('td', class_ = 'calendar__previous').text
			forecast = news.find_next_sibling('td', class_ = 'calendar__forecast').text
			actual = news.find_next_sibling('td', class_ = 'calendar__actual').text
			event_time = news.text.strip()
			compare = ''
			if(actual != None and  actual != ''):
				compareClass = news.find_next_sibling('td', class_ = 'calendar__actual').find_next('span').get('class')
				if(len(compareClass) > 0):
					compare = compareClass[0]

			try:
				matchObj = re.search('([0-9]+)(:[0-9]{2})([a|p]m)', event_time) # Regex to match time in the format HH:MMam/pm
				if(matchObj != None):
					event_time_hour = matchObj.group(1) # Matches the first group in the regex which is the hour in HH format
					event_time_minutes = matchObj.group(2) # Matches the second group in the regex which is the minutes in :MM format 
					am_or_pm = matchObj.group(3) # Matches the third group in the regex which is either 'am' or 'pm'
				elif(re.search('All Day', event_time)):
					event_time_hour = '0'
					event_time_minutes = ':00'
					am_or_pm = 'am'
				elif(re.search('Day [0-9]+', event_time)):
					#event_time_hour = '0'
					#event_time_minutes = ':00'
					#am_or_pm = 'am'
					with open(file_path, 'a') as file:
						file.write('{}, {}, {}, {}, {}, {}, {}, {}, {}, {}\n'.format(day_of_week, event_date_time, event_time_holder, curr, impact, event, previous, forecast, actual, compare))
					continue
				else:
					# else no time and use previous events time and write to file
					with open(file_path, 'a') as file:
						file.write('{}, {}, {}, {}, {}, {}, {}, {}, {}, {}\n'.format(day_of_week, event_date_time, event_time_holder, curr, impact, event, previous, forecast, actual, compare))
					continue
					
				adjusted_date_time = timeDateAdjust(event_time_hour, event_time_minutes, am_or_pm, 0, year, month, day) # Returns a tuple with 3 elements consisting of 'event date YYYY:MM:DD', 'event time HH:MM', 'day of week Mon-Fri'

				event_date = adjusted_date_time[0]
				event_time = adjusted_date_time[1]
				day_of_week = adjusted_date_time[2]

				if event_time != '' and event_time != 'All Day': # If the event time is not empy and not 'All day' then we have found a time 
					event_time_holder = str(adjusted_date_time[1]) # Set the event_time_holder to this event_time so any subsequent events also have the same time as this event
																   # As forex factory only provides a time for the first event
					event_date_time = '{} {}'.format(event_date, event_time_holder) #
				else:
					event_time_holder = event_time_holder # event_time_holder remains the same and should have the value of the first event which was assigned a time
					event_date_time = '{} {}'.format(event_date, event_time_holder) 

			except Exception as e:
				print("There was an error: " + e)

			print(file_path)
			# print('{}, {}, {}, {}, {}, {}, {}, {}, {}\n'.format(day_of_week, event_date_time, event_time_holder, curr, impact, event, previous, forecast, actual))
			with open(file_path, 'a') as file:
				file.write('{}, {}, {}, {}, {}, {}, {}, {}, {}, {}\n'.format(day_of_week, event_date_time, event_time_holder, curr, impact, event, previous, forecast, actual, compare))

	if start_date == end_date:
		print('Successfully retrieved all data')
		return True
	else:
		#scrape_next_day = soup.findAll('li', class_='left pagination')
		#a = scrape_next_day.find_next('a', class_=' calendar__pagination--next')
		#
		#link = scrape_next_day['href']
		start = start + timedelta(days=1)
		getEventsCalendar(start, end, file_path)


def strToIntMonth(month):
	#
	# Function to convert Str Month into an Int
	#

	if(month == 'Jan'):
		return 1
	elif(month == "Feb"):
		return 2
	elif(month == "Mar"):
		return 3
	elif(month == "Apr"):
		return 4
	elif(month == "May"):
		return 5
	elif(month == "Jun"):
		return 6
	elif(month == "Jul"):
		return 7
	elif(month == "Aug"):
		return 8
	elif(month == "Sep"):
		return 9
	elif(month == "Oct"):
		return 10
	elif(month == "Nov"):
		return 11
	elif(month == "Dec"):
		return 12
	else:
		return None

def timeDateAdjust(event_time_hour, event_time_minutes, am_or_pm, hours_to_adjust, year, month, day):

	d = date(year, month, day)

	if(am_or_pm == "am"):
		if(int(event_time_hour) == 12):
			adjusted_hour = 0
		else:
			adjusted_hour = int(event_time_hour) + hours_to_adjust # Hours_to_adjust variable is used to adjust for timzone differences as the forex factory calendar is in EST
	else:
		if(int(event_time_hour) == 12):
			adjusted_hour = 12
		else:
			adjusted_hour = int(event_time_hour) + 12 + hours_to_adjust # If pm then add 12 hours to adjust to 24 hours format
	
	# If adjusted_hour < 24 hours no need to update the date 
	# if it is over 24 then this means that it is the next day and the date needs to be updated.

	if(adjusted_hour < 24):
		adjusted_time = str(adjusted_hour) + event_time_minutes # Returns string representation of the 24h time in HH:MM
		d_of_week = calendar.day_abbr[d.weekday()] # use the calendar API to return Mon-Sun in abbreviated format as a string
		d= d.strftime("%Y.%m.%d") # Returns the date as a string in the format YYYY:MM:DD
		return (d, adjusted_time, d_of_week)
	else:
		adjusted_hour = adjusted_hour - 24 # Minus 24h as it is now the next day and 24h time will be am of the next day
		adjusted_time = str(adjusted_hour) + event_time_minutes # Returns string representation of the 24h time in HH:MM
		d = d + timedelta(days=1) # Adds one day on the original date of the event
		d_of_week = calendar.day_abbr[d.weekday()] # use the calendar API to return Mon-Sun in abbreviated format as a string
		d= d.strftime("%Y.%m.%d") # Returns the date as a string in the format YYYY:MM:DD
		return (d, adjusted_time, d_of_week)

	d = date(year, month, day)
	
if __name__ == "__main__":
   
	abs_path = os.path.abspath(__file__)
	cwd = os.path.dirname(abs_path)
	parent_dir = os.path.dirname(cwd) 
	if len(sys.argv) >= 3:
		file_name = sys.argv[1]
		startStr = sys.argv[2]
		endStr = sys.argv[3]
	else:
		file_name = ""
		startStr = ""
		endStr = ""

	if startStr != None and startStr != "":
		start = datetime.strptime(startStr,'%Y%m%d').date()
	else:
		start = date(2024,1,1)

	if endStr != None and endStr != "":
		end = datetime.strptime(endStr,'%Y%m%d').date()
	else:
		end = date(2024,4,22)

	if file_name != None and file_name != "":
		file_path = cwd + "\\" + file_name
	else:
		file_path = cwd + "\\ffc_news_events.csv"
	
	if os.path.exists(file_path):
	# Delete the file
		os.remove(file_path)
		print(f"old {file_path} has been deleted.")
	else:
		print(f"{file_path} does not exist.")
		os.makedirs(os.path.dirname(file_path), exist_ok=True)

	with open(file_path, 'a+') as file:
		file.write("") # Needs to write an empty line so that file is opened and getEventsCalendar can append to the file

	
	getEventsCalendar(start,end, file_path)

	

