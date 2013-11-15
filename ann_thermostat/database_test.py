#!/usr/bin/python
# -*- coding: utf-8 -*-
from pyfann import libfann
import MySQLdb as mdb
import datetime
import random
con = mdb.connect('192.168.1.3', 'kolaf', 'darumbar', 'homeseer')
f = '%Y-%m-%d %H:%M:%S'
cases = {}
confidence = 1
lookahead = 4
temperatures= [16]*36+[17]*36+ [16]*36+ [15]*36+[17.5]*36+[20]*36+[16]*36+ [17.5]*36+ [21]*36+ [22]*36+[27]*24+[12]*24
import collections

d = collections.deque(temperatures)
k={}

def normalise (number, maximum):
    return float (number)/maximum

def calculate_state(heater_on,start_temperature, heating_rate, schedule):
    print "----------------------------------------------------------"
    print "heating_rate: " +str (heating_rate)
    print "start_temperature: "  +str(start_temperature)
    print "heater: " +str (heater_on)
    print schedule
    rate =float(heating_rate)/12
    for counter in range(len(schedule)):
        current_temperature = start_temperature+ (counter*rate)
        if current_temperature <= schedule [counter] - confidence or (current_temperature <= schedule [counter] + confidence and heater_on == 1):# and current_temperature >= schedule [counter] - confidence:
            print "inside the interval at index " +str(counter) 
            return "1"
        if counter >12*lookahead:
            print "Outside lookahead interval, remaining off"
            return "0"
    print "remaining off"
    return "0"
    
with con:

    cur = con.cursor(mdb.cursors.DictCursor)
    cur.execute("SELECT * FROM learning")

    rows = cur.fetchall()

    for row in rows:
        case = {}
        inputs = {}
        print "Measurements for learning: " + row["room"] + " " + str(row["span"])+ " " + str(row ["timestamp_hours"])+ "\n"
        cur = con.cursor(mdb.cursors.DictCursor)
        command  ="""select * from readings m inner join 
        (
        select devicename, MAX(date) as timestamp from (select * from readings where
        date < DATE_SUB(str_to_date('""" + str (row ["timestamp_hours"]) +  """','%Y-%m-%d %T' ),INTERVAL """ + str(row["span"]) + """ HOUR) )as  Victor
        group by devicename
        ) r on m.devicename = r.devicename and m.date = r.timestamp order by m.devicename
        """
        
        cur.execute (command)
        measurements =cur.fetchall()
        case["room"]=row["room"]
        case["heating_rate"]=row["heating_rate"]
        case["cooling_rate"]=row["cooling_rate"]
        case["indoor_start"] =row["indoor_start"]
        print "Number of measurements: " +str (len(measurements))
        for measurement in measurements:
            
            previous_command ="select * from readings where deviceID =" + str (measurement["deviceID"]) + " and lastchange < str_to_date('" + str (measurement["lastchange"]) +  "','%Y-%m-%d %T' ) order by lastchange desc limit 1"
            #print previous_command
            cur = con.cursor(mdb.cursors.DictCursor)
            cur.execute (previous_command)
            previous = cur.fetchone()
            if previous:
                inputs[measurement["devicename"]] =  measurement["number"]
                latest_reading =measurement["lastchange"]
                inputs["previous_" +previous["devicename"]] =  previous["number"]
                
                prior_reading =previous ["lastchange"]
                ts=(latest_reading-prior_reading)
                
                inputs["interval_"+previous["devicename"]] = ts.days * 1440 + float(ts.seconds)/60
        # print inputs.keys ()
        if len (inputs) >0 and case["heating_rate"]:
            if not case ["room"] in k:
                k[case ["room"]] = set (inputs.keys())
                
            else:
                k[case ["room"]]&=set(inputs.keys())
            case["input"]=inputs
            if not case ["room"] in cases:
                cases[case ["room"]] = []
            cases[case ["room"]].append (case)
        # print k[case ["room"]]
        
repetitions = 30
for room, training in cases.items ():
    
    test_cases = len(training)*repetitions*2
    keys=list(k[room])
    keys.sort()
    # print keys
    print  room
    input_num=len(keys)+12*lookahead+1
    output = open ('training_' + room +'.txt','w')
    output_keys = open ('keys_' + room +'.txt','w')
    output.write (str(test_cases) + " " + str(input_num)+ " 1\n" )
    output_keys.write (",".join (keys))
    output_keys.close()
    for c in training:
        if c["heating_rate"]:
            for heater in range (2):
                for i in range(repetitions):
                    
                    #keys =c.keys()
                    #keys.sort()
                    string = str (heater)  + " "
                    string =string +" ".join(str(c["input"][f]) for f in keys)
                    # print string
                    d.rotate (random.randint (2,len (d)))
                    string = string + " " + " ".join (str(u) for u in list (d)[:12*lookahead])
                    output.write (string + "\n" )
                    output.write (calculate_state (heater,c ["indoor_start"],c ["heating_rate"],d) + "\n")
                    #print answer
    output.close ()
    
for room in cases.keys():
    num_neurons_hidden = 4
    num_output = 1

    desired_error = 0.0001
    max_neurons = 40
    neurons_between_reports = 1
    steepnesses = [0.1,0.2,0.4,0.5,0.6,0.7,0.8,0.9,1.0,1.1]

    train_data = libfann.training_data()
    train_data.read_train_from_file('training_' + room +'.txt')

    train_data.scale_train_data(0, 1)

    ann = libfann.neural_net()
    ann.create_shortcut_array([len(train_data.get_input()[0]), len(train_data.get_output()[0])])

    ann.set_training_algorithm(libfann.TRAIN_RPROP);



    ann.set_activation_function_hidden(libfann.SIGMOID_SYMMETRIC);
    ann.set_activation_function_output(libfann.LINEAR_PIECE);
    ann.set_activation_steepness_hidden(0.5);
    ann.set_activation_steepness_output(0.5);

    ann.set_train_error_function(libfann.ERRORFUNC_LINEAR);

    ann.set_rprop_increase_factor(1.2);
    ann.set_rprop_decrease_factor(0.5);
    ann.set_rprop_delta_min(0.0);
    ann.set_rprop_delta_max(50.0);

    ann.set_cascade_output_change_fraction(0.01);
    ann.set_cascade_output_stagnation_epochs(12);
    ann.set_cascade_candidate_change_fraction(0.01);
    ann.set_cascade_candidate_stagnation_epochs(12);
    ann.set_cascade_weight_multiplier(0.4);
    ann.set_cascade_candidate_limit(1000.0);
    ann.set_cascade_max_out_epochs(150);
    ann.set_cascade_max_cand_epochs(150);
    ann.set_cascade_activation_steepnesses(steepnesses);
    ann.set_cascade_num_candidate_groups(1);


    ann.print_parameters();


    ann.cascadetrain_on_data(train_data, max_neurons, neurons_between_reports, desired_error);

    ann.print_connections();

    print "\nTrain error: %f\n" %( ann.test_data(train_data) )


    ann.save('fann_' + room +'.txt');    