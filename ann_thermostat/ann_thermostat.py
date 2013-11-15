#!/usr/bin/python
# -*- coding: utf-8 -*-

import MySQLdb as mdb
import datetime
import random
import sys
con = mdb.connect('192.168.1.3', 'kolaf', 'darumbar', 'homeseer')
f = '%Y-%m-%d %H:%M:%S'
cases = {}
confidence = 1
lookahead = 4
#temperatures= [16]*36+[17]*36+ [16]*36+ [15]*36+[17.5]*36+[20]*36+[16]*36+ [17.5]*36+ [21]*36+ [22]*36
import collections
from pyfann import libfann

room_name=sys.argv[1]
heater_state =sys.argv[2]
temperatures = [float(d) for d in sys.argv[3:]]
d = collections.deque(temperatures)


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
keys = input_keys.readline().split(",")
keys.sort()
# d.rotate (random.randint (2,len (d)))
input = [float(heater_state)] + [float(case[f]) for f in keys] + list (d)[:12*lookahead]
#print len(input)
# print input
print ann.run (input)[0]                   