#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'd6bc8605-9ebe-4ab7-9208-2fcf8d05d8d9'

apiUrl = "api/IoC/org/#{orgId}/action/GetIoCById/#{id}"
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
