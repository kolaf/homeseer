#!/usr/bin/python
# -*- coding: utf-8 -*-

import MySQLdb as mdb
import datetime
import random
import sys
con = mdb.connect('192.168.1.3', 'homeseer', 'homeseer', 'homeseer')
f = '%Y-%m-%d %H:%M:%S'
cases = {}

lookahead = 4

from pyfann import libfann

room_name=sys.argv[1]
confidence=sys.argv[2]
heater_state =int(sys.argv[3])
current_temperature =float (sys.argv[4])
temperatures = [float(d) for d in sys.argv[5:]]

def calculate_state(heating_rate,start_temperature, heater_on,schedule):
    print  >> sys.stderr, "----------------------------------------------------------\n"
    print  >> sys.stderr, "heating_rate: " +str (heating_rate)+"\n"
    print  >> sys.stderr, schedule
    print  >> sys.stderr, "start temp " + str(start_temperature) +"\n"
    rate =float(heating_rate)/12
    for counter in range(len(schedule)):
        current_temperature = start_temperature+ (counter*rate)
        if current_temperature <= schedule [counter] - confidence or (current_temperature <= schedule [counter] + confidence and heater_on == 1):# and current_temperature >= schedule [counter] - confidence:
            # print >> sys.stderr, "inside the interval at index " +str(counter) 
            return "1.0"
        if counter >12*lookahead:
            # print >> sys.stderr, "Outside lookahead interval, remaining off"
            return "0.0"
    # print >> sys.stderr, "remaining off"
    return "0.0"


with con:
    
    case = {}
    cur = con.cursor(mdb.cursors.DictCursor)
    command  ="""select * from readings m inner join 
        (
        select devicename, MAX(lastchange) as timestamp from readings
        group by devicename
        ) r on m.devicename = r.devicename and m.lastchange = r.timestamp order by m.devicename
        """
    cur.execute (command)
    measurements =cur.fetchall()
    # print "Number of measurements: " +str (len(measurements))
    case ["maximum_temperature"] = max (temperatures)
    for measurement in measurements:
        
        previous_command ="select * from readings where deviceID ='" + str (measurement["deviceID"])  + "' and lastchange < str_to_date('" + str (measurement["lastchange"]) +  "','%Y-%m-%d %T' )  order by lastchange desc limit 1"
        #print previous_command
        cur = con.cursor(mdb.cursors.DictCursor)
        cur.execute (previous_command)
        previous = cur.fetchone()
        if previous:
            case[measurement["devicename"]] =  measurement["number"]
            latest_reading =measurement["lastchange"]
            case["previous_" +previous["devicename"]] =  previous["number"]
            
            prior_reading =previous ["lastchange"]
            ts=(latest_reading-prior_reading)
            
            case["interval_"+previous["devicename"]] = ts.days * 1440 + float(ts.seconds)/60
    # print inputs.keys ()
    
ann=libfann.neural_net()
ann.create_from_file("ann_thermostat/fann_"+room_name+".txt")
input_keys = open ("ann_thermostat/keys_" + room_name + ".txt")    

scaling_file = open ("ann_thermostat/scaling_" + room_name + ".txt")
maximum_list = scaling_file.readline().split(",")
minimum_list = scaling_file.readline().split(",")
maximum = {}
minimum = {}
for thing in maximum_list:
    key, value = thing.split (":")
    maximum [key] = float(value)
for thing in minimum_list:
    key, value = thing.split (":")
    minimum [key] = float(value)

for key, value in case.items ():
        # print key
        # print "value ", value
        # print "maximum ", maximum [key]
        # print "minimum ", minimum [key]
        
        number = 1
        if maximum [key] != minimum [key]:
            number = float(value - minimum [key])/(maximum [key] - minimum [key])
        # print "number  before correction ", number
        if number >1:
            number = 1
        if number <0:
            number = 0
        # print "number ", number
        case[key] = number
    
# ann.create_from_file("fann_"+room_name+".txt")
# input_keys = open ("keys_" + room_name + ".txt")      
# print  >> sys.stderr, str(case)
keys = input_keys.readline().split(",")
keys.sort()
# d.rotate (random.randint (2,len (d)))
input = [float(case[f]) for f in keys]
#print len(input)
output =ann.run (input)[0]
print  >> sys.stderr, str(output)
heating_rate =  output*(maximum ["heating_rate"] - minimum ["heating_rate"]) + minimum ["heating_rate"]
print calculate_state (heating_rate, current_temperature, heater_state,temperatures)
