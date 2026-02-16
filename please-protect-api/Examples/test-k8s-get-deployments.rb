#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Proxy/org/#{orgId}/action/Kube/apis/apps/v1/deployments" #statefulsets or daemonsets or replicasets
param = nil

result = make_request(:get, apiUrl, param)

json = result.to_json
puts(json)
