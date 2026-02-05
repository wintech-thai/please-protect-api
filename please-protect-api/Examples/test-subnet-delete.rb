#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'f224b7c2-d82f-49a2-a323-e9a064ca3e5d'

apiUrl = "api/Subnet/org/#{orgId}/action/DeleteSubnetById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
