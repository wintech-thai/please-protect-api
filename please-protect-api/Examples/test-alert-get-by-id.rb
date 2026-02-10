#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = '07061bfc-da04-47a0-80b3-612d4b0b326b'

apiUrl = "api/AlertEvent/org/#{orgId}/action/GetAlertEventById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)
json = result.to_json
puts(json)
