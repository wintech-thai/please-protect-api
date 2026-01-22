#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = '5b237da7-5212-4dae-b29a-5ca48aeae79d'

apiUrl = "api/IoC/org/#{orgId}/action/DeleteIoCById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
