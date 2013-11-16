#!/usr/bin/python
# -*- coding: utf-8 -*-
from pyfann import libfann
import json
import MySQLdb as mdb
import datetime
import random
room_temperature_map = {'BedroomSE':262, 'BedroomN':7772, 'Livingroom':9533}
room_heater_map = {'BedroomSE':7679, 'BedroomN':2356, 'Livingroom':8379}
con = mdb.connect('192.168.1.3', 'homeseer', 'homeseer', 'homeseer')
f = '%Y-%m-%d %H:%M:%S'
cases = {}
k={}

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
    for room, thermometer in room_temperature_map.items():
        heater = room_heater_map [room]
        cur = con.cursor(mdb.cursors.DictCursor)
        cur.execute("SELECT * FROM readings where  deviceID ='" + str(heater) + "' and lastchange > str_to_date('2013-11-11 08:01:01','%Y-%m-%d %T') order by lastchange asc")
        rows = cur.fetchall()
        heating_start = None
        heating_stop = None
        cur.close()
        for row in rows:
            if row["number"] ==  100:
                cur = con.cursor(mdb.cursors.DictCursor)
                cur.execute("SELECT * FROM readings where  deviceID ='" + str(thermometer) + "' and  lastchange  <= str_to_date('" + str (row ["lastchange"]) +  "','%Y-%m-%d %T') order by lastchange desc ")
                heating_start = cur.fetchone()
                # print "heating start temperature at " + str(heating_start ["lastchange"]) + " is " + str(heating_start ["number"])
                heating_stop=None
                cur.close()
            if row["number"] ==  0 and heating_start:
                cur2 = con.cursor(mdb.cursors.DictCursor)
                cur2.execute("SELECT * FROM readings where  deviceID ='" + str(thermometer) + "' and  lastchange  <= str_to_date('" + str (row ["lastchange"]) +  "','%Y-%m-%d %T') order by lastchange desc ")
                heating_stop = cur2.fetchone()
                # print "heating stop temperature at " + str(heating_stop ["lastchange"]) + " is " + str(heating_stop ["number"])
                cur2.close()
            if heating_start  and heating_stop and heating_start !=  heating_stop:
                ts=(heating_stop ["lastchange"]-heating_start ["lastchange"])
                # print "recording heating period"
                # print heating_stop
                # print heating_start
                maximum_temperature =heating_stop ["number"]
                heating_rate =  (heating_stop ["number"] -heating_start ["number"])/(ts.days * 24 + float(ts.seconds)/3600)
                print "room: " + room + ", heating rates: " + str(heating_rate) + " heating start: " +str( heating_start ["lastchange"]) + " heating stop: " + str(heating_stop ["lastchange"])
                
                case = {}
                
                if heating_rate >0:
                    print "Recording heating rate"
                    command  ="""select * from readings m inner join 
                        (
                        select devicename, MAX(lastchange) as timestamp from (select * from readings where
                        lastchange  <= str_to_date('""" + str (heating_stop ["lastchange"]) +  """','%Y-%m-%d %T')) as  Victor
                        group by devicename
                        ) r on m.devicename = r.devicename and m.lastchange = r.timestamp order by m.devicename
                        """
                    # print command
                    cur = con.cursor(mdb.cursors.DictCursor)
                    cur.execute (command)
                    heating_start_measurements =cur.fetchall()
                    
                    
                    
                    case [ "room"] = room
                    case ["heating_rate"] = heating_rate
                    
                    inputs = {}
                    inputs ["maximum_temperature"]= maximum_temperature
                    for measurement in heating_start_measurements:
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
                    if len (inputs) >0:
                        if not case ["room"] in k:
                            k[case ["room"]] = set (inputs.keys())
                            
                        else:
                            k[case ["room"]]&=set(inputs.keys())
                        case["input"]=inputs
                        if not case ["room"] in cases:
                            cases[case ["room"]] = []
                        cases[case ["room"]].append (case)
                    # print k[case ["room"]]
                    # print json.dumps(case, indent = 1, sort_keys = True)
                heating_start = None
                heating_stop = None 
        
# scale
for room, training in cases.items ():
    keys=list(k[room])
    keys.sort()
    maximum = {}
    minimum = {}
    for train in training:
        for key, value in train["input"].items ():
            if not key in maximum:
                maximum [key] = 0
                minimum [key] = 1000
            maximum [key] =max(maximum [key], value)
            minimum [key] =min(minimum [key], value)
    
        if not "heating_rate" in maximum:
            maximum ["heating_rate"] = 0
            minimum ["heating_rate"] = 1000
        maximum ["heating_rate"] =max(maximum ["heating_rate"], train["heating_rate"])
        minimum ["heating_rate"] =min(minimum ["heating_rate"], train["heating_rate"])
    print maximum
    print minimum
    for train in training:
        for key, value in train["input"].items ():
            number = 1
            if maximum [key] != minimum [key]:
                number = float(value - minimum [key])/(maximum [key] - minimum [key])
            if number >1:
                number = 1
            if number <0:
                number = 0
            train["input"] [key] = number
        key="heating_rate"
        number = float(train["heating_rate"] - minimum [key])/(maximum [key] - minimum [key])
        train["heating_rate"] = number
    output = open ('scaling_' + room +'.txt','w')
    keys = maximum.keys ()
    keys.sort ()
    output.write (",".join([a+ ":" + str(maximum [a]) for a in  keys]  )+ "\n")
    output.write (",".join([a+ ":" + str(minimum [a]) for a in  keys]  )+ "\n")
    output.close ()
    
    
repetitions = 1
for room, training in cases.items ():
    
    test_cases = len(training)*repetitions
    keys=list(k[room])
    keys.sort()
    # print keys
    print  room
    input_num=len(keys)
    output = open ('training_' + room +'.txt','w')
    output_keys = open ('keys_' + room +'.txt','w')
    output.write (str(test_cases) + " " + str(input_num)+ " 1\n" )
    output_keys.write (",".join (keys))
    output_keys.close()
    for c in training:
        
        for i in range(repetitions):
            string =" ".join(str(c["input"][f]) for f in keys)
            print string
            output.write (string + "\n" )
            output.write (str(c["heating_rate"]) + "\n")
            #print answer
    output.close ()
    
for room in cases.keys():
    # num_neurons_hidden = 26
    # num_output = 1
    connection_rate = 1
    # learning_rate = 0.7
    # num_neurons_hidden = 26
    
    # max_iterations = 100
    # iterations_between_reports = 50

    desired_error = 0.000001
    max_neurons = 100
    neurons_between_reports = 1
    # steepnesses = [0.1,0.2,0.4,0.5,0.6,0.7,0.8,0.9,1.0,1.1]

    train_data = libfann.training_data()
    train_data.read_train_from_file('training_' + room +'.txt')

    train_data.scale_train_data(0, 1)

    ann = libfann.neural_net()
    ann.create_shortcut_array([len(train_data.get_input()[0]), len(train_data.get_output()[0])])

    # ann.create_sparse_array(connection_rate, (len(train_data.get_input()[0]), num_neurons_hidden, len(train_data.get_output()[0])))
    # ann.set_learning_rate(learning_rate)


    # start training the network
    print "Training network"
    # ann.set_activation_function_hidden(libfann.SIGMOID_SYMMETRIC_STEPWISE)
    # ann.set_activation_function_output(libfann.SIGMOID_STEPWISE)
    # ann.set_training_algorithm(libfann.TRAIN_INCREMENTAL)
        
    # ann.train_on_data(train_data, max_iterations, iterations_between_reports, desired_error)

    
    
    ann.set_training_algorithm(libfann.TRAIN_RPROP);



    ann.set_activation_function_hidden(libfann.SIGMOID_SYMMETRIC);
    ann.set_activation_function_output(libfann.ELLIOT_SYMMETRIC);
    # ann.set_activation_steepness_hidden(0.5);
    # ann.set_activation_steepness_output(0.5);

    # ann.set_train_error_function(libfann.ERRORFUNC_LINEAR);

    # ann.set_rprop_increase_factor(1.2);
    # ann.set_rprop_decrease_factor(0.5);
    # ann.set_rprop_delta_min(0.0);
    # ann.set_rprop_delta_max(50.0);

    # ann.set_cascade_output_change_fraction(0.01);
    # ann.set_cascade_output_stagnation_epochs(12);
    # ann.set_cascade_candidate_change_fraction(0.01);
    # ann.set_cascade_candidate_stagnation_epochs(12);
    # ann.set_cascade_weight_multiplier(0.4);
    # ann.set_cascade_candidate_limit(1000.0);
    # ann.set_cascade_max_out_epochs(150);
    # ann.set_cascade_max_cand_epochs(150);
    # ann.set_cascade_activation_steepnesses(steepnesses);
    # ann.set_cascade_num_candidate_groups(3);


    # ann.print_parameters();


    ann.cascadetrain_on_data(train_data, max_neurons, neurons_between_reports, desired_error);

    #ann.print_connections();

    print "\nTrain error: %f\n" %( ann.test_data(train_data) )


    ann.save('fann_' + room +'.txt');    
