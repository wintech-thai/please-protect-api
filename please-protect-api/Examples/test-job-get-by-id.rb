#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'd4df0bbc-7292-4261-aa8e-73cb0b768a25'

apiUrl = "api/Job/org/#{orgId}/action/GetJobById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
